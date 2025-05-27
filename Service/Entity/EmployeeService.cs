using CRMService.API;
using CRMService.DataBase.Postgresql;
using CRMService.Interfaces.Repository;
using CRMService.Models.ConfigClass;
using CRMService.Models.Entity;
using Microsoft.Extensions.Options;
using System.Data;

namespace CRMService.Service.Entity
{
    public class EmployeeService(IOptions<ApiEndpoint> endpoint, IOptions<OkdeskSettings> okdeskSettings, IUnitOfWorkEntities unitOfWork, PGSelect pGSelect, GetItemService _request, ILoggerFactory logger)
    {
        private readonly ILogger<EmployeeService> _logger = logger.CreateLogger<EmployeeService>();

        private async IAsyncEnumerable<List<Employee>?> GetEmployeesFromCloudApi(long startIndex, long limit)
        {
            string link = $"{endpoint.Value.OkdeskApi}/employees/list?api_token={okdeskSettings.Value.ApiToken}";

            await foreach (var employees in _request.GetAllItems<Employee>(link, startIndex, limit))
                yield return employees;

        }

        private async Task<List<Employee>?> GetEmployeesFromCloudDb(int startIndex, int limit)
        {
            string sqlCommand = string.Format("SELECT * FROM users " +
                "WHERE type = 'Employee' AND users.sequential_id > '{0}' " +
                "ORDER BY users.sequential_id " +
                "LIMIT '{1}';", startIndex, limit);

            DataSet ds = await pGSelect.Select(sqlCommand);
            DataTable? employeesTable = ds.Tables["Table"];
            if (employeesTable == null)
                return null;

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
                    Login = employee.Field<string>("login")
                }).ToList();
        }

        public async Task UpdateEmployeesFromCloudApi(long startIndex, long limit)
        {
            _logger.LogInformation("[Method:{MethodName}] Starting updating employees.", nameof(UpdateEmployeesFromCloudApi));

            await foreach (List<Employee>? employees in GetEmployeesFromCloudApi(startIndex, limit))
            {
                if (employees == null || employees.Count == 0)
                    return;

                await unitOfWork.Employee.CreateOrUpdate(employees);

                await unitOfWork.SaveAsync();

                await UpdateConnectionsBetweenEmployeesAndRoles(employees);
            }

            _logger.LogInformation("[Method:{MethodName}] Employees update completed.", nameof(UpdateEmployeesFromCloudApi));
        }

        public async Task UpdateEmployeesFromCloudDb(int startIndexEmployee, int limit)
        {
            while (true)
            {
                List<Employee>? employees = await GetEmployeesFromCloudDb(startIndexEmployee, limit);

                if (employees == null || employees.Count == 0)
                    return;

                startIndexEmployee = employees.Last().Id;

                await unitOfWork.Employee.CreateOrUpdate(employees);

                await unitOfWork.SaveAsync();
            }
        }

        private async Task UpdateConnectionsBetweenEmployeesAndRoles(List<Employee> employees)
        {
            foreach (Employee employee in employees)
            {
                if (employee.Roles == null || !employee.Roles.Any())
                    continue;

                foreach (Role role in employee.Roles)
                {
                    if (string.IsNullOrWhiteSpace(role.Name)) continue;

                    Role? roleFromDb = await unitOfWork.Role.GetRoleByName(role.Name);

                    if (roleFromDb == null) continue;
                    // Понадобится для удаления
                    role.Id = roleFromDb.Id;

                    EmployeeRole connection = new() { EmployeeId = employee.Id, RoleId = roleFromDb.Id };

                    if (await unitOfWork.EmployeeRole.GetItem(connection, false) == null)
                        unitOfWork.EmployeeRole.Create(connection);
                }
            }
            await unitOfWork.SaveAsync();

            await DeleteIrrelevantConnectionsEmployeeRoleFromCloudApi(employees);
        }

        private async Task DeleteIrrelevantConnectionsEmployeeRoleFromCloudApi(List<Employee> employees)
        {
            foreach (Employee employee in employees)
            {
                if (employee.Roles == null || !employee.Roles.Any())
                    continue;

                IEnumerable<EmployeeRole>? localConnections = await unitOfWork.EmployeeRole.GetConnectionsByEmployee(employee.Id);

                if (localConnections == null || !localConnections.Any())
                    continue;

                foreach (var connection in localConnections)
                {
                    // Если связи нет в облачном API, но она есть в локальной БД
                    if (employee.Roles.FirstOrDefault(c => c.Id == connection.RoleId) == null)
                        // Удалить такую связь т.к. она больше неактуальна
                        unitOfWork.EmployeeRole.Delete(connection);
                }
            }

            await unitOfWork.SaveAsync();
        }
    }
}