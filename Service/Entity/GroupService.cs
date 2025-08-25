using CRMService.API;
using CRMService.DataBase.Postgresql;
using CRMService.Interfaces.Repository;
using CRMService.Models.ConfigClass;
using CRMService.Models.Entity;
using Microsoft.Extensions.Options;
using System.Data;

namespace CRMService.Service.Entity
{
    public class GroupService(IOptions<ApiEndpoint> endpoint, IOptions<OkdeskSettings> okdeskSettings, GetItemService request, IUnitOfWork unitOfWork, PGSelect pGSelect, ILoggerFactory logger)
    {
        private readonly ILogger<GroupService> _logger = logger.CreateLogger<GroupService>();

        private async Task<List<Group>?> GetGroupsFromCloudApi()
        {
            string link = endpoint.Value.OkdeskApi + "/employees/groups?api_token=" + okdeskSettings.Value.ApiToken;

            return await request.GetRangeOfItems<Group>(link);
        }

        private async Task<List<Group>?> GetGroupsFromCloudDb()
        {
            string sqlCommand = "SELECT * FROM groups ORDER BY groups.sequential_id;";

            DataSet ds = await pGSelect.Select(sqlCommand);
            DataTable? groupTable = ds.Tables["Table"];
            if (groupTable == null)
                return null;

            return groupTable.AsEnumerable().
                Select(group => new Group
                {
                    Id = group.Field<int>("sequential_id"),
                    Name = group.Field<string>("name")
                }).ToList();
        }

        private async Task<List<EmployeeGroup>?> GetConnectionsFromCloudDb()
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
                return null;

            return groupTable.AsEnumerable().
                Select(group => new EmployeeGroup
                {
                    Id = groupTable.Rows.IndexOf(group) + 1,
                    EmployeeId = group.Field<int>("userId"),
                    GroupId = group.Field<int>("groupId")
                }).ToList();
        }

        public async Task UpdateGroupsFromCloudApi()
        {
            _logger.LogInformation("[Method:{MethodName}] Starting updating groups.", nameof(UpdateGroupsFromCloudApi));

            List<Group>? groups = await GetGroupsFromCloudApi();

            if (groups == null || groups.Count == 0) return;

            await unitOfWork.Group.CreateOrUpdate(groups);

            await unitOfWork.SaveAsync();

            await UpdateConnectionsWithEmployeesAndGroupsFromCloudApi(groups);

            _logger.LogInformation("[Method:{MethodName}] Groups update completed.", nameof(UpdateGroupsFromCloudApi));
        }

        public async Task UpdateGroupsFromCloudDb()
        {
            List<Group>? groups = await GetGroupsFromCloudDb();

            if (groups == null || groups.Count == 0)
                return;

            await unitOfWork.Group.CreateOrUpdate(groups);

            await unitOfWork.SaveAsync();

            await UpdateConnectionsWithEmployeesAndGroupsFromCloudDb();
        }

        private async Task UpdateConnectionsWithEmployeesAndGroupsFromCloudApi(List<Group> groups)
        {
            foreach (var group in groups)
            {
                if (group.Employees == null || !group.Employees.Any()) continue;

                foreach (var employee in group.Employees)
                {
                    EmployeeGroup? connection = new() { EmployeeId = employee.Id, GroupId = group.Id };
                    if (await unitOfWork.EmployeeGroup.GetConnectionByEmployeeAndGroup((int)connection.EmployeeId, (int)connection.GroupId) == null)
                        unitOfWork.EmployeeGroup.Create(connection);
                }
            }

            await unitOfWork.SaveAsync();

            await DeleteIrrelevantConnectionsEmployeeGroupFromCloudApi(groups);
        }

        private async Task UpdateConnectionsWithEmployeesAndGroupsFromCloudDb()
        {
            List<EmployeeGroup>? connectionsFromCloudDb = await GetConnectionsFromCloudDb();

            if (connectionsFromCloudDb == null || connectionsFromCloudDb.Count == 0)
                return;

            await unitOfWork.EmployeeGroup.CreateOrUpdate(connectionsFromCloudDb);

            await unitOfWork.SaveAsync();

            await DeleteIrrelevantConnectionsEmployeeGroupFromCloudDb(connectionsFromCloudDb);
        }

        private async Task DeleteIrrelevantConnectionsEmployeeGroupFromCloudDb(List<EmployeeGroup> connectionsFromCloudDb)
        {
            IEnumerable<EmployeeGroup>? localConnections = await unitOfWork.EmployeeGroup.GetItems(startIndex: 0, await unitOfWork.EmployeeGroup.GetCountOfItems());

            if (localConnections == null || !localConnections.Any())
                return;

            foreach (var connection in localConnections)
            {
                if (connectionsFromCloudDb.FirstOrDefault(c => c.EmployeeId == connection.EmployeeId && c.GroupId == connection.GroupId) == null)
                    unitOfWork.EmployeeGroup.Delete(connection);
            }

            await unitOfWork.SaveAsync();
        }

        private async Task DeleteIrrelevantConnectionsEmployeeGroupFromCloudApi(List<Group> groups)
        {
            foreach (Group group in groups)
            {
                IEnumerable<EmployeeGroup>? localConnections = await unitOfWork.EmployeeGroup.GetConnectionsByGroup(group.Id);

                if (localConnections == null || !localConnections.Any())
                    continue;

                foreach (var connection in localConnections)
                {
                    // Если связи нет в облачном API, но она есть в локальной БД
                    if (group.Employees?.FirstOrDefault(c => c.Id == connection.EmployeeId) == null)
                        // Удалить такую связь т.к. она больше неактуальна
                        unitOfWork.EmployeeGroup.Delete(connection);
                }
            }

            await unitOfWork.SaveAsync();
        }
    }
}
