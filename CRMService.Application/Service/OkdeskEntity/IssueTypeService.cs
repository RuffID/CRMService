using CRMService.Application.Abstractions.Database.Repository;
using CRMService.Application.Common.Mapping.OkdeskEntity;
using CRMService.Application.Models.ConfigClass;
using CRMService.Application.Service.Sync;
using CRMService.Contracts.Models.Dto.Lookup;
using CRMService.Contracts.Models.Dto.OkdeskEntity;
using CRMService.Contracts.Models.Request;
using CRMService.Contracts.Models.Responses;
using CRMService.Contracts.Models.Responses.Results;
using CRMService.Domain.Models.OkdeskEntity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CRMService.Application.Service.OkdeskEntity
{
    public class IssueTypeService(IOptions<ApiEndpointOptions> endpoint, IOptions<OkdeskOptions> okdeskSettings, IOkdeskEntityRequestService request, IUnitOfWork unitOfWork, IOkdeskUnitOfWork okdeskUnitOfWork, EntitySyncService sync, ILogger<IssueTypeService> logger)
    {
        private const int DEFAULT_LOOKUP_LIMIT = 20;

        public async Task<ServiceResult<List<TaskTypeDto>>> GetTypes(CancellationToken ct)
        {
            List<IssueType> types = await unitOfWork.IssueType.GetItemsByPredicateAsync(asNoTracking: true,
                include: t => t.Include(x => x.Group),
                ct: ct);

            return ServiceResult<List<TaskTypeDto>>.Ok(types.ToDto().ToList());
        }

        public async Task<ServiceResult<List<IssueTypeGroupDto>>> GetTypeGroups(CancellationToken ct)
        {
            List<IssueTypeGroup> types = await unitOfWork.IssueTypeGroup.GetItemsByPredicateAsync(asNoTracking: true, ct: ct);

            return ServiceResult<List<IssueTypeGroupDto>>.Ok(types.ToDto().ToList());
        }

        public async Task<ServiceResult<List<LookupOptionDto>>> GetTypeLookupAsync(LookupListRequest requestModel, CancellationToken ct = default)
        {
            ServiceResult validationResult = ValidateLookupRequest(requestModel);
            if (!validationResult.Success)
                return ServiceResult<List<LookupOptionDto>>.Fail(validationResult.Error!.StatusCode, validationResult.Error.Message);

            string? normalizedSearch = NormalizeSearch(requestModel.Search);

            List<IssueType> types = await unitOfWork.IssueType.GetItemsByPredicateAsync(
                predicate: type => normalizedSearch == null || (type.Name != null && type.Name.Contains(normalizedSearch)),
                asNoTracking: true,
                ct: ct);

            IEnumerable<IssueType> orderedTypes = normalizedSearch == null
                ? types.OrderBy(type => type.Id)
                : types
                    .OrderBy(type => type.Name != null && type.Name.StartsWith(normalizedSearch, StringComparison.OrdinalIgnoreCase) ? 0 : 1)
                    .ThenBy(type => type.Name)
                    .ThenBy(type => type.Id);

            List<LookupOptionDto> items = orderedTypes
                .Skip(requestModel.Offset)
                .Take(requestModel.Limit)
                .Select(type => new LookupOptionDto
                {
                    Id = type.Id,
                    Text = type.Name ?? $"#{type.Id}"
                })
                .ToList();

            return ServiceResult<List<LookupOptionDto>>.Ok(items);
        }

        public async Task<(List<IssueType> Types, List<IssueTypeGroup> TypeGroups)> GetIssueTypesFromCloudApi(CancellationToken ct)
        {
            string link = $"{endpoint.Value.OkdeskApi}/dictionaries/issues/types?api_token={okdeskSettings.Value.OkdeskApiToken}";

            List<IssueTypeResponse> root = await request.GetRangeOfItemsAsync<IssueTypeResponse>(link, ct: ct);
            List<IssueType> types = new List<IssueType>();
            List<IssueTypeGroup> groups = new List<IssueTypeGroup>();

            foreach (IssueTypeResponse type in root)
                TypeHandler(type, null, groups, types);

            return (types, groups);
        }

        public async Task<List<IssueType>> GetIssueTypesFromCloudDb(CancellationToken ct)
        {
            List<IssueType> issueTypes = await okdeskUnitOfWork.IssueType.GetItemsByPredicateAsync(asNoTracking: true, ct: ct);

            return issueTypes.OrderBy(x => x.Id).ToList();
        }

        public async Task<List<IssueTypeGroup>> GetIssueTypeGroupsFromCloudDb(CancellationToken ct)
        {
            List<IssueTypeGroup> issueTypes = await okdeskUnitOfWork.IssueTypeGroup.GetItemsByPredicateAsync(asNoTracking: true, ct: ct);

            return issueTypes.OrderBy(x => x.Id).ToList();
        }

        public async Task UpdateIssueTypesFromCloudApi(CancellationToken ct)
        {
            logger.LogInformation("[Method:{MethodName}] Starting to update issue types from API.", nameof(UpdateIssueTypesFromCloudApi));

            (List<IssueType> Types, List<IssueTypeGroup> Groups) = await GetIssueTypesFromCloudApi(ct);

            if (Types.Count == 0 && Groups.Count == 0)
                return;

            foreach (IssueType item in Types)
            {
                await sync.RunExclusive(item, async () =>
                {
                    IssueType? existingTypes = await unitOfWork.IssueType.GetItemByPredicateAsync(predicate: t => t.Code == item.Code, ct: ct);

                    if (existingTypes == null)
                        unitOfWork.IssueType.Create(item);
                    else
                        existingTypes.CopyData(item);

                    await unitOfWork.SaveChangesAsync(ct);
                }, ct);
            }

            foreach (IssueTypeGroup item in Groups)
            {
                await sync.RunExclusive(item, async () =>
                {
                    IssueTypeGroup? existingTypes = await unitOfWork.IssueTypeGroup.GetItemByPredicateAsync(predicate: t => t.Code == item.Code, ct: ct);

                    if (existingTypes == null)
                        unitOfWork.IssueTypeGroup.Create(item);
                    else
                        existingTypes.CopyData(item);

                    await unitOfWork.SaveChangesAsync(ct);
                }, ct);
            }

            logger.LogInformation("[Method:{MethodName}] Update issue types completed.", nameof(UpdateIssueTypesFromCloudApi));
        }

        public async Task UpdateIssueTypesFromCloudDb(CancellationToken ct)
        {
            logger.LogInformation("[Method:{MethodName}] Starting to update issue types.", nameof(UpdateIssueTypesFromCloudDb));

            List<IssueType> types = await GetIssueTypesFromCloudDb(ct);
            List<IssueTypeGroup> typeGroups = await GetIssueTypeGroupsFromCloudDb(ct);

            foreach (IssueTypeGroup item in typeGroups)
            {
                await sync.RunExclusive(item, async () =>
                {
                    IssueTypeGroup? existingTypes = await unitOfWork.IssueTypeGroup.GetItemByIdAsync(item.Id, asNoTracking: true, ct: ct);

                    if (existingTypes == null)
                        unitOfWork.IssueTypeGroup.Create(item);
                    else
                        existingTypes.CopyData(item);

                    await unitOfWork.SaveChangesAsync(ct);
                }, ct);
            }

            foreach (IssueType item in types)
            {
                await sync.RunExclusive(item, async () =>
                {
                    IssueType? existingTypes = await unitOfWork.IssueType.GetItemByIdAsync(item.Id, asNoTracking: true, ct: ct);
                    if (existingTypes == null)
                        unitOfWork.IssueType.Create(item);
                    else
                        existingTypes.CopyData(item);

                    await unitOfWork.SaveChangesAsync(ct);
                }, ct);
            }            

            logger.LogInformation("[Method:{MethodName}] Update issue types completed.", nameof(UpdateIssueTypesFromCloudDb));
        }

        private void TypeHandler(IssueTypeResponse node, int? parentGroupId, List<IssueTypeGroup> groups, List<IssueType> types)
        {
            bool isGroup = string.Equals(node.Type, "group", StringComparison.OrdinalIgnoreCase);

            if (isGroup)
            {
                IssueTypeGroup group = new IssueTypeGroup
                {
                    Id = node.Id,
                    Code = node.Code,
                    Name = node.Name,
                    ParentGroupId = parentGroupId
                };
                groups.Add(group);

                if (node.Children != null)
                {
                    foreach (IssueTypeResponse child in node.Children)
                        TypeHandler(child, node.Id, groups, types);
                }
            }
            else
            {
                IssueType type = new()
                {
                    Id = node.Id,
                    Code = node.Code,
                    Name = node.Name,
                    IsDefault = node.Default,
                    IsInner = node.Inner,
                    AvailableForClient = node.AvailableForClient,
                    GroupId = parentGroupId
                };
                types.Add(type);
            }
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
