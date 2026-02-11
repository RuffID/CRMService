using CRMService.Abstractions.Database.Repository;
using CRMService.DataBase.Postgresql;
using CRMService.Models.ConfigClass;
using CRMService.Models.Dto.Mappers.OkdeskEntity;
using CRMService.Models.Dto.OkdeskEntity;
using CRMService.Models.OkdeskEntity;
using CRMService.Models.Responses.Results;
using CRMService.Service.Requests;
using Microsoft.Extensions.Options;
using System.Data;

namespace CRMService.Service.OkdeskEntity
{
    public class GroupService(IOptions<ApiEndpointOptions> endpoint, IOptions<OkdeskOptions> okdeskSettings, GetOkdeskEntityService request, IUnitOfWork unitOfWork, PGSelect pGSelect, ILogger<GroupService> logger)
    {
        public async Task<ServiceResult<List<GroupDto>>> GetGroups(CancellationToken ct = default)
        {
            List<Group> groups = await unitOfWork.Group.GetItemsByPredicateAsync(asNoTracking: true, ct: ct);

            return ServiceResult<List<GroupDto>>.Ok(groups.ToDto().ToList());
        }

        public async Task<List<Group>> GetGroupsFromCloudApi(CancellationToken ct)
        {
            string link = endpoint.Value.OkdeskApi + "/employees/groups?api_token=" + okdeskSettings.Value.OkdeskApiToken;

            return await request.GetRangeOfItems<Group>(link, ct: ct);
        }

        private async Task<List<Group>> GetGroupsFromCloudDb(CancellationToken ct)
        {
            string sqlCommand = "SELECT * FROM groups ORDER BY groups.sequential_id;";

            DataSet ds = await pGSelect.Select(sqlCommand, ct);
            DataTable? groupTable = ds.Tables["Table"];
            if (groupTable == null)
                return new();

            return groupTable.AsEnumerable().
                Select(group => new Group
                {
                    Id = group.Field<int>("sequential_id"),
                    Name = group.Field<string>("name") ?? ""
                }).ToList();
        }

        public async Task UpdateGroupsFromCloudApi(CancellationToken ct)
        {
            logger.LogInformation("[Method:{MethodName}] Starting to update groups from API.", nameof(UpdateGroupsFromCloudApi));

            List<Group> groups = await GetGroupsFromCloudApi(ct);

            if (groups.Count == 0) return;

            foreach (Group group in groups)
            {
                group.Employees?.Clear();

                Group? existingGroup = await unitOfWork.Group.GetItemByIdAsync(group.Id, ct: ct);

                if (existingGroup == null)
                    unitOfWork.Group.Create(group);
                else
                {
                    existingGroup.CopyData(group);
                }
            }

            await unitOfWork.SaveAsync(ct);
        }

        public async Task UpdateGroupsFromCloudDb(CancellationToken ct)
        {
            logger.LogInformation("[Method:{MethodName}] Starting to update groups from API.", nameof(UpdateGroupsFromCloudDb));

            List<Group> groups = await GetGroupsFromCloudDb(ct);

            if (groups.Count == 0)
                return;

            foreach (Group group in groups)
            {
                Group? existingGroup = await unitOfWork.Group.GetItemByIdAsync(group.Id, ct: ct);
                if (existingGroup == null)
                    unitOfWork.Group.Create(group);
                else
                    existingGroup.CopyData(group);
            }

            await unitOfWork.SaveAsync(ct);
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
                return;

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
                return;

            unitOfWork.EmployeeGroup.CreateRange(toAdd);

            unitOfWork.EmployeeGroup.DeleteRange(toDelete);

            await unitOfWork.SaveAsync(ct);
        }
    }
}
