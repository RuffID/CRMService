using CRMService.API;
using CRMService.DataBase.Postgresql;
using CRMService.Interfaces.Repository;
using CRMService.Models.ConfigClass;
using CRMService.Models.Entity;
using Microsoft.Extensions.Options;
using System.Data;

namespace CRMService.Service.Entity
{
    public class EmployeeService(IOptions<ApiEndpointOptions> endpoint, IOptions<OkdeskOptions> okdeskSettings, IUnitOfWork unitOfWork, PGSelect pGSelect, GetOkdeskEntityService _request, ILoggerFactory logger)
    {
        private readonly ILogger<EmployeeService> _logger = logger.CreateLogger<EmployeeService>();

        private async IAsyncEnumerable<List<Employee>?> GetEmployeesFromCloudApi(long startIndex, long limit)
        {
            string link = $"{endpoint.Value.OkdeskApi}/employees/list?api_token={okdeskSettings.Value.OkdeskApiToken}";

            await foreach (var employees in _request.GetAllItems<Employee>(link, startIndex, limit))
                yield return employees;
        }

        private async Task<List<Employee>> GetEmployeesFromCloudDb(int startIndex, int limit)
        {
            string sqlCommand = string.Format("SELECT * FROM users " +
                "WHERE type = 'Employee' AND users.sequential_id > '{0}' " +
                "ORDER BY users.sequential_id " +
                "LIMIT '{1}';", startIndex, limit);

            DataSet ds = await pGSelect.Select(sqlCommand);
            DataTable? employeesTable = ds.Tables["Table"];
            if (employeesTable == null)
                return new ();

            return employeesTable.AsEnumerable().
                Select(employee => new Employee
                {
                    Id = employee.Field<int>("sequential_id"),
                    FirstName = employee.Field<string>("first_name"),
                    LastName = employee.Field<string>("last_name"),
                    Patronymic = employee.Field<string>("patronymic"),
                    Position = employee.Field<string>("position"),
                    Active = employee.Field<bool>("active"),
                    Email = employee.Field<string>("email"),
                    Login = employee.Field<string>("login") ?? string.Empty
                }).ToList();
        }

        public async Task UpdateEmployeesFromCloudApi(long startIndex, long limit, CancellationToken ct)
        {
            _logger.LogInformation("[Method:{MethodName}] Starting updating employees.", nameof(UpdateEmployeesFromCloudApi));

            await foreach (List<Employee>? employees in GetEmployeesFromCloudApi(startIndex, limit))
            {
                if (employees == null || employees.Count == 0)
                    break;

                await unitOfWork.Employee.Upsert(employees, ct);

                await unitOfWork.SaveAsync(ct);

                //await UpdateConnectionsBetweenEmployeesAndRoles(employees, ct);
            }

            _logger.LogInformation("[Method:{MethodName}] Employees update completed.", nameof(UpdateEmployeesFromCloudApi));
        }

        public async Task UpdateEmployeesFromCloudDb(int startIndexEmployee, int limit, CancellationToken ct)
        {
            while (true)
            {
                List<Employee>? employees = await GetEmployeesFromCloudDb(startIndexEmployee, limit);

                if (employees == null || employees.Count == 0)
                    break;

                startIndexEmployee = employees.Last().Id;

                await unitOfWork.Employee.Upsert(employees, ct);

                await unitOfWork.SaveAsync(ct);
            }
        }

        private async Task UpdateConnectionsBetweenEmployeesAndRoles(List<Employee> employees, CancellationToken ct)
        {
            foreach (Employee employee in employees)
            {
                if (employee.Roles == null || employee.Roles.Count == 0)
                    continue;

                foreach (OkdeskRole role in employee.Roles)
                {
                    if (string.IsNullOrWhiteSpace(role.Name)) 
                        continue;

                    OkdeskRole? roleFromDb = await unitOfWork.OkdeskRole.GetItemByPredicate(r => r.Name == role.Name, asNoTracking: true, ct: ct);

                    if (roleFromDb == null) 
                        continue;

                    EmployeeRole connection = new() { EmployeeId = employee.Id, RoleId = roleFromDb.Id };

                    if (await unitOfWork.EmployeeRole.GetItemByPredicate(predicate: er => er.EmployeeId == employee.Id && er.RoleId == roleFromDb.Id, asNoTracking: false, ct) == null)
                        unitOfWork.EmployeeRole.Create(connection);
                }
            }

            await unitOfWork.SaveAsync(ct);

            await DeleteIrrelevantConnectionsEmployeeRoleFromCloudApi(employees, ct);
        }

        private async Task DeleteIrrelevantConnectionsEmployeeRoleFromCloudApi(List<Employee> employees, CancellationToken ct)
        {
            foreach (Employee employee in employees)
            {
                if (employee.Roles == null || employee.Roles.Count == 0)
                    continue;

                List<EmployeeRole> localConnections = await unitOfWork.EmployeeRole.GetItemsByPredicate(predicate: er => er.EmployeeId == employee.Id, asNoTracking: false, ct: ct);

                if (localConnections == null || localConnections.Count == 0)
                    continue;

                foreach (EmployeeRole connection in localConnections)
                {
                    // Если связи нет в облачном API, но она есть в локальной БД
                    if (employee.Roles.FirstOrDefault(c => c.Id == connection.RoleId) == null)
                        // Удалить такую связь т.к. она больше неактуальна
                        unitOfWork.EmployeeRole.Delete(connection);
                }
            }

            await unitOfWork.SaveAsync(ct);
        }
    }
}