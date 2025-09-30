using CRMService.API;
using CRMService.DataBase.Postgresql;
using CRMService.Interfaces.Repository;
using CRMService.Models.ConfigClass;
using CRMService.Models.OkdeskEntity;
using Microsoft.Extensions.Options;
using System.Data;

namespace CRMService.Service.OkdeskEntity
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

        public sealed class EmployeeRoleKeyComparer : IEqualityComparer<EmployeeRole>
        {
            public bool Equals(EmployeeRole? x, EmployeeRole? y)
            {
                if (ReferenceEquals(x, y))
                    return true;

                if (x is null || y is null)
                    return false;

                return x.EmployeeId == y.EmployeeId && x.RoleId == y.RoleId;
            }

            public int GetHashCode(EmployeeRole obj)
            {
                return HashCode.Combine(obj.EmployeeId, obj.RoleId);
            }
        }

        private async Task UpsertEmployeeRolesConnectionsFromApi(List<Employee> employees, CancellationToken ct)
        {
            // Отфильтровать пустые наборы ролей сразу
            List<Employee> employeesWithRoles = employees.Where(e => e.Roles != null && e.Roles.Count > 0).ToList();

            if (employeesWithRoles.Count == 0)
                return;

            // Собрать уникальные имена ролей из API
            HashSet<string> roleNames = employeesWithRoles
                .SelectMany(e => e.Roles!)
                .Where(r => !string.IsNullOrWhiteSpace(r.Name))
                .Select(r => r.Name)
                .ToHashSet(StringComparer.Ordinal);

            if (roleNames.Count == 0)
                return;

            // Предзагрузить роли из БД разом по именам
            List<OkdeskRole> rolesFromDb = await unitOfWork.OkdeskRole.GetItemsByPredicate(r => roleNames.Contains(r.Name), asNoTracking: true, ct: ct);

            // Построить целевой список связей
            List<EmployeeRole> desired = new List<EmployeeRole>();
            foreach (Employee employee in employeesWithRoles)
            {
                foreach (OkdeskRole apiRole in employee.Roles!)
                {
                    if (string.IsNullOrWhiteSpace(apiRole.Name))
                        continue;

                    OkdeskRole? localRole = rolesFromDb.FirstOrDefault(r => r.Name == apiRole.Name);
                    if (localRole == null)
                        unitOfWork.OkdeskRole.Create(apiRole);

                    desired.Add(new EmployeeRole
                    {
                        EmployeeId = employee.Id,
                        RoleId = apiRole.Id
                    });
                }
            }

            if (desired.Count == 0)
                return;

            HashSet<int> employeeIds = new (employeesWithRoles.Select(e => e.Id));

            List<EmployeeRole> existing = await unitOfWork.EmployeeRole.GetItemsByPredicate(er => employeeIds.Contains(er.EmployeeId), asNoTracking: true, ct: ct);

            EmployeeRoleKeyComparer cmp = new ();

            List<EmployeeRole> toAdd = desired.Except(existing, cmp).ToList();

            List<EmployeeRole> toDelete = existing.Except(desired, cmp).ToList();

            if (toAdd.Count == 0 && toDelete.Count == 0)
                return;

            await unitOfWork.ExecuteInTransaction(async () =>
            {
                if (toAdd.Count > 0)
                    foreach (var item in toAdd)
                        unitOfWork.EmployeeRole.Create(item);   // Добавить связи
                        

                if (toDelete.Count > 0)
                    foreach (var item in toDelete)
                        unitOfWork.EmployeeRole.Delete(item);   // Удалить неактуальные

                await Task.CompletedTask;
            }, ct);
        }

    }
}
