using CRMService.API;
using CRMService.DataBase.Postgresql;
using CRMService.Interfaces.Repository;
using CRMService.Models.ConfigClass;
using CRMService.Models.OkdeskEntity;
using Microsoft.Extensions.Options;
using System.Data;

namespace CRMService.Service.OkdeskEntity
{
    public class GroupService(IOptions<ApiEndpointOptions> endpoint, IOptions<OkdeskOptions> okdeskSettings, GetOkdeskEntityService request, IUnitOfWork unitOfWork, PGSelect pGSelect)
    {
        public async Task<List<Group>> GetGroupsFromCloudApi()
        {
            string link = endpoint.Value.OkdeskApi + "/employees/groups?api_token=" + okdeskSettings.Value.OkdeskApiToken;

            return await request.GetRangeOfItems<Group>(link);
        }

        private async Task<List<Group>> GetGroupsFromCloudDb()
        {
            string sqlCommand = "SELECT * FROM groups ORDER BY groups.sequential_id;";

            DataSet ds = await pGSelect.Select(sqlCommand);
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
            List<Group> groups = await GetGroupsFromCloudApi();

            if (groups.Count == 0) return;

            await unitOfWork.Group.Upsert(groups, ct);

            await unitOfWork.SaveAsync(ct);
        }

        public async Task UpdateGroupsFromCloudDb(CancellationToken ct)
        {
            List<Group> groups = await GetGroupsFromCloudDb();

            if (groups.Count == 0)
                return;

            await unitOfWork.Group.Upsert(groups, ct);

            await unitOfWork.SaveAsync(ct);
        }

        public async Task UpsertEmployeeGroupConnectionsFromApi(CancellationToken ct)
        {
            List<Group> groups = await GetGroupsFromCloudApi();

            List<int> groupIds = groups.Select(g => g.Id).Distinct().ToList();

            List<Employee> allEmployeesFromApi = groups.SelectMany(g => g.Employees ?? Enumerable.Empty<Employee>())
                .DistinctBy(e => e.Id)
                .ToList();

            List<int> employeeIds = allEmployeesFromApi.Select(e => e.Id).Distinct().ToList();

            List<EmployeeGroup> desired = new ();

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

            List<EmployeeGroup> existing = await unitOfWork.EmployeeGroup
                .GetItemsByPredicate(
                    eg => employeeIds.Contains(eg.GroupId) && groupIds.Contains(eg.GroupId), 
                    asNoTracking: true, 
                    ct: ct);

            List<EmployeeGroup> toAdd = desired.Except(existing, EmployeeGroup.Comparer).ToList();

            List<EmployeeGroup> toDelete = existing.Except(desired, EmployeeGroup.Comparer).ToList();

            if (toAdd.Count == 0 && toDelete.Count == 0)
                return;

            await unitOfWork.ExecuteInTransaction(async () =>
            {
                if (toAdd.Count > 0)
                {
                    foreach (var item in toAdd)
                        unitOfWork.EmployeeGroup.Create(item);
                }

                if (toDelete.Count > 0)
                {
                    foreach (var item in toDelete)
                        unitOfWork.EmployeeGroup.Delete(item);
                }

                await Task.CompletedTask;
            }, ct);
        }
    }
}
