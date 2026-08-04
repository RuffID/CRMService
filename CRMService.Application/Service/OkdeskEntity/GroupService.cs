using CRMService.Application.Abstractions.Database.Repository;
using CRMService.Application.Common.Mapping.OkdeskEntity;
using CRMService.Application.Models.ConfigClass;
using CRMService.Application.Service.Sync;
using CRMService.Contracts.Models.Dto.Lookup;
using CRMService.Contracts.Models.Dto.OkdeskEntity;
using CRMService.Contracts.Models.Request;
using CRMService.Contracts.Models.Responses.Results;
using CRMService.Domain.Models.OkdeskEntity;
using Microsoft.Extensions.Options;

namespace CRMService.Application.Service.OkdeskEntity
{
    public class GroupService(IOptions<ApiEndpointOptions> endpoint, IOptions<OkdeskOptions> okdeskSettings, IOkdeskEntityRequestService request, IUnitOfWork unitOfWork, IOkdeskUnitOfWork okdeskUnitOfWork, EntitySyncService sync, ILogger<GroupService> logger)
    {
        private const int DEFAULT_LOOKUP_LIMIT = 20;

        public async Task<ServiceResult<List<GroupDto>>> GetGroups(CancellationToken ct = default)
        {
            List<Group> groups = await unitOfWork.Group.GetItemsByPredicateAsync(asNoTracking: true, ct: ct);

            return ServiceResult<List<GroupDto>>.Ok(groups.ToDto().ToList());
        }

        public async Task<ServiceResult<List<LookupOptionDto>>> GetGroupLookupAsync(LookupListRequest requestModel, CancellationToken ct = default)
        {
            ServiceResult validationResult = ValidateLookupRequest(requestModel);
            if (!validationResult.Success)
                return ServiceResult<List<LookupOptionDto>>.Fail(validationResult.Error!.StatusCode, validationResult.Error.Message);

            string? normalizedSearch = NormalizeSearch(requestModel.Search);

            List<Group> groups = await unitOfWork.Group.GetItemsByPredicateAsync(
                predicate: group => normalizedSearch == null || (group.Name != null && group.Name.Contains(normalizedSearch)),
                asNoTracking: true,
                ct: ct);

            IEnumerable<Group> orderedGroups = normalizedSearch == null
                ? groups.OrderBy(group => group.Id)
                : groups
                    .OrderBy(group => group.Name != null && group.Name.StartsWith(normalizedSearch, StringComparison.OrdinalIgnoreCase) ? 0 : 1)
                    .ThenBy(group => group.Name)
                    .ThenBy(group => group.Id);

            List<LookupOptionDto> items = orderedGroups
                .Skip(requestModel.Offset)
                .Take(requestModel.Limit)
                .Select(group => new LookupOptionDto
                {
                    Id = group.Id,
                    Text = group.Name ?? $"#{group.Id}"
                })
                .ToList();

            return ServiceResult<List<LookupOptionDto>>.Ok(items);
        }

        public async Task<List<Group>> GetGroupsFromCloudApi(CancellationToken ct)
        {
            string link = endpoint.Value.OkdeskApi + "/employees/groups?api_token=" + okdeskSettings.Value.OkdeskApiToken;

            return await request.GetRangeOfItemsAsync<Group>(link, ct: ct);
        }

        private async Task<List<Group>> GetGroupsFromCloudDb(CancellationToken ct)
        {
            List<Group> groups = await okdeskUnitOfWork.Group.GetItemsByPredicateAsync(asNoTracking: true, ct: ct);

            return groups.OrderBy(x => x.Id).ToList();
        }

        public async Task UpdateGroupsFromCloudApi(CancellationToken ct)
        {
            logger.LogInformation("[Method:{MethodName}] Starting to update groups from API.", nameof(UpdateGroupsFromCloudApi));

            List<Group> groups = await GetGroupsFromCloudApi(ct);

            if (groups.Count != 0)
            {
                foreach (Group group in groups)
                {
                    await sync.RunExclusive(group, async () =>
                    {
                        group.Employees?.Clear();

                        Group? existingGroup = await unitOfWork.Group.GetItemByIdAsync(group.Id, ct: ct);

                        if (existingGroup == null)
                            unitOfWork.Group.Create(group);
                        else
                            existingGroup.CopyData(group);

                        await unitOfWork.SaveChangesAsync(ct);
                    }, ct);
                }
            }

            logger.LogInformation("[Method:{MethodName}] Update groups completed.", nameof(UpdateGroupsFromCloudApi));
        }

        public async Task UpdateGroupsFromCloudDb(CancellationToken ct)
        {
            logger.LogInformation("[Method:{MethodName}] Starting to update groups from API.", nameof(UpdateGroupsFromCloudDb));

            List<Group> groups = await GetGroupsFromCloudDb(ct);

            if (groups.Count != 0)
            {
                foreach (Group group in groups)
                {
                    await sync.RunExclusive(group, async () =>
                    {
                        Group? existingGroup = await unitOfWork.Group.GetItemByIdAsync(group.Id, ct: ct);
                        if (existingGroup == null)
                            unitOfWork.Group.Create(group);
                        else
                            existingGroup.CopyData(group);

                        await unitOfWork.SaveChangesAsync(ct);
                    }, ct);
                }
            }

            logger.LogInformation("[Method:{MethodName}] Update groups completed.", nameof(UpdateGroupsFromCloudDb));
        }

        public async Task UpsertEmployeeGroupConnectionsFromApi(CancellationToken ct)
        {
            logger.LogInformation("[Method:{MethodName}] Starting to update employee-group connections from API.", nameof(UpsertEmployeeGroupConnectionsFromApi));

            List<Group> groups = await GetGroupsFromCloudApi(ct);

            List<Employee> allEmployeesFromApi = groups.SelectMany(g => g.Employees ?? Enumerable.Empty<Employee>())
                .DistinctBy(e => e.Id)
                .ToList();

            List<EmployeeGroup> desired = new();

            foreach (Group group in groups)
            {
                foreach (Employee employee in group.Employees)
                {
                    desired.Add(new EmployeeGroup
                    {
                        EmployeeId = employee.Id,
                        GroupId = group.Id
                    });
                }
            }

            if (desired.Count == 0)
            {
                logger.LogInformation("[Method:{MethodName}] Employee-group connections update completed. Groups: {GroupCount}, employees: {EmployeeCount}, desired: {DesiredCount}, added: {AddedCount}, deleted: {DeletedCount}.",
                    nameof(UpsertEmployeeGroupConnectionsFromApi), groups.Count, allEmployeesFromApi.Count, desired.Count, 0, 0);

                return;
            }

            List<int> groupIds = groups.Select(g => g.Id).Distinct().ToList();
            List<int> employeeIds = allEmployeesFromApi.Select(e => e.Id).Distinct().ToList();

            List<EmployeeGroup> existing = await unitOfWork.EmployeeGroup
                .GetItemsByPredicateAsync(
                    eg => employeeIds.Contains(eg.EmployeeId) && groupIds.Contains(eg.GroupId),
                    asNoTracking: true,
                    ct: ct);

            List<EmployeeGroup> toAdd = desired.Except(existing, EmployeeGroup.Comparer).ToList();
            List<EmployeeGroup> toDelete = existing.Except(desired, EmployeeGroup.Comparer).ToList();

            if (toAdd.Count == 0 && toDelete.Count == 0)
            {
                logger.LogInformation("[Method:{MethodName}] Employee-group connections update completed. Groups: {GroupCount}, employees: {EmployeeCount}, desired: {DesiredCount}, added: {AddedCount}, deleted: {DeletedCount}.",
                    nameof(UpsertEmployeeGroupConnectionsFromApi), groups.Count, allEmployeesFromApi.Count, desired.Count, 0, 0);

                return;
            }

            unitOfWork.EmployeeGroup.CreateRange(toAdd);
            unitOfWork.EmployeeGroup.DeleteRange(toDelete);

            await unitOfWork.SaveChangesAsync(ct);

            logger.LogInformation("[Method:{MethodName}] Employee-group connections update completed. Groups: {GroupCount}, employees: {EmployeeCount}, desired: {DesiredCount}, added: {AddedCount}, deleted: {DeletedCount}.",
                nameof(UpsertEmployeeGroupConnectionsFromApi), groups.Count, allEmployeesFromApi.Count, desired.Count, toAdd.Count, toDelete.Count);
        }

        private static ServiceResult ValidateLookupRequest(LookupListRequest request)
        {
            if (request.Offset < 0)
                return ServiceResult.Fail(400, "Смещение не может быть отрицательным.");

            if (request.Limit == 0)
                request.Limit = DEFAULT_LOOKUP_LIMIT;

            if (request.Limit <= 0 || request.Limit > 100)
                return ServiceResult.Fail(400, "Лимит должен быть в диапазоне от 1 до 100.");

            return ServiceResult.Ok();
        }

        private static string? NormalizeSearch(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            string normalized = value.Trim();
            int nonWhitespaceCount = normalized.Count(character => !char.IsWhiteSpace(character));
            return nonWhitespaceCount >= 2 ? normalized : null;
        }
    }
}
