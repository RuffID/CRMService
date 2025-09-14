using CRMService.API;
using CRMService.DataBase.Postgresql;
using CRMService.Interfaces.Repository;
using CRMService.Models.ConfigClass;
using CRMService.Models.Entity;
using Microsoft.Extensions.Options;
using System.Data;

namespace CRMService.Service.Entity
{
    public class GroupService(IOptions<ApiEndpointOptions> endpoint, IOptions<OkdeskOptions> okdeskSettings, GetOkdeskEntityService request, IUnitOfWork unitOfWork, PGSelect pGSelect, ILoggerFactory logger)
    {
        private readonly ILogger<GroupService> _logger = logger.CreateLogger<GroupService>();

        private async Task<List<Group>> GetGroupsFromCloudApi()
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

        private async Task<List<EmployeeGroup>> GetConnectionsFromCloudDb()
        {
            string sqlCommand =
                "SELECT employee_groups.id, users.sequential_id AS userId, groups.sequential_id AS groupId " +
                "FROM employee_groups " +
                "LEFT OUTER JOIN users ON employee_groups.employee_id = users.id " +
                "LEFT OUTER JOIN groups ON employee_groups.group_id = groups.id " +
                "WHERE type = 'Employee' " +
                "ORDER BY employee_groups.id;";

            DataSet ds = await pGSelect.Select(sqlCommand);
            DataTable? groupTable = ds.Tables["Table"];
            if (groupTable == null)
                return new();

            return groupTable.AsEnumerable().
                Select(group => new EmployeeGroup
                {
                    EmployeeId = group.Field<int>("userId"),
                    GroupId = group.Field<int>("groupId")
                }).ToList();
        }

        public async Task UpdateGroupsFromCloudApi(CancellationToken ct)
        {
            _logger.LogInformation("[Method:{MethodName}] Starting updating groups.", nameof(UpdateGroupsFromCloudApi));

            List<Group> groups = await GetGroupsFromCloudApi();

            if (groups.Count == 0) return;

            await unitOfWork.Group.Upsert(groups, ct);

            await unitOfWork.SaveAsync(ct);

            //await UpdateConnectionsWithEmployeesAndGroupsFromCloudApi(groups, ct);

            _logger.LogInformation("[Method:{MethodName}] Groups update completed.", nameof(UpdateGroupsFromCloudApi));
        }

        public async Task UpdateGroupsFromCloudDb(CancellationToken ct)
        {
            List<Group> groups = await GetGroupsFromCloudDb();

            if (groups.Count == 0)
                return;

            await unitOfWork.Group.Upsert(groups, ct);

            await unitOfWork.SaveAsync(ct);

            //await UpdateConnectionsWithEmployeesAndGroupsFromCloudDb(ct);
        }

        private async Task UpdateConnectionsWithEmployeesAndGroupsFromCloudApi(List<Group> groups, CancellationToken ct)
        {
            foreach (Group group in groups)
            {
                if (group.Employees == null || group.Employees.Count == 0) 
                    continue;

                foreach (var employee in group.Employees)
                {
                    EmployeeGroup? connection = new() { EmployeeId = employee.Id, GroupId = group.Id };
                    EmployeeGroup? existingConnection = await unitOfWork.EmployeeGroup.GetItemByPredicate(predicate: eg => eg.EmployeeId == employee.Id && eg.GroupId == group.Id, asNoTracking: true, ct: ct);

                    if (existingConnection == null)
                        unitOfWork.EmployeeGroup.Create(connection);
                }
            }

            await DeleteIrrelevantConnectionsEmployeeGroupFromCloudApi(groups, ct);
        }

        private async Task UpdateConnectionsWithEmployeesAndGroupsFromCloudDb(CancellationToken ct)
        {
            List<EmployeeGroup> connectionsFromCloudDb = await GetConnectionsFromCloudDb();

            if (connectionsFromCloudDb.Count == 0)
                return;

            await unitOfWork.EmployeeGroup.Upsert(connectionsFromCloudDb,
                eg => (EmployeeGroup c) => c.EmployeeId == eg.EmployeeId && c.GroupId == eg.GroupId,
                ct);

            await unitOfWork.SaveAsync(ct);

            await DeleteIrrelevantConnectionsEmployeeGroupFromCloudDb(connectionsFromCloudDb, ct);
        }

        private async Task DeleteIrrelevantConnectionsEmployeeGroupFromCloudDb(List<EmployeeGroup> connectionsFromCloudDb, CancellationToken ct)
        {
            List<EmployeeGroup>? localConnections = await unitOfWork.EmployeeGroup.GetItemsByPredicate(asNoTracking: true, ct: ct);

            if (localConnections.Count == 0)
                return;

            foreach (EmployeeGroup connection in localConnections)
            {
                EmployeeGroup? exist = connectionsFromCloudDb.FirstOrDefault(c => c.EmployeeId == connection.EmployeeId && c.GroupId == connection.GroupId);
                if (exist == null)
                    unitOfWork.EmployeeGroup.Delete(connection);
            }

            await unitOfWork.SaveAsync(ct);
        }

        private async Task DeleteIrrelevantConnectionsEmployeeGroupFromCloudApi(List<Group> groups, CancellationToken ct)
        {
            foreach (Group group in groups)
            {
                List<EmployeeGroup> localConnections = await unitOfWork.EmployeeGroup.GetItemsByPredicate(predicate: eg => eg.GroupId == group.Id, asNoTracking: true, ct: ct);

                if (localConnections.Count == 0)
                    continue;

                foreach (EmployeeGroup connection in localConnections)
                {
                    // Если связи нет в облачном API, но она есть в локальной БД

                    if (group.Employees?.FirstOrDefault(c => c.Id == connection.EmployeeId) == null)
                        // Удалить такую связь т.к. она больше неактуальна
                        unitOfWork.EmployeeGroup.Delete(connection);
                }
            }

            await unitOfWork.SaveAsync(ct);
        }
    }
}
