using CRMService.API;
using CRMService.DataBase.Postgresql;
using CRMService.Interfaces.Repository;
using CRMService.Models.ConfigClass;
using CRMService.Models.OkdeskEntity;
using Microsoft.Extensions.Options;
using System.Data;

namespace CRMService.Service.OkdeskEntity
{
    public class EmployeeService(IOptions<ApiEndpointOptions> endpoint, IOptions<OkdeskOptions> okdeskSettings, IUnitOfWork unitOfWork, PGSelect pGSelect, GetOkdeskEntityService request)
    {
        public async IAsyncEnumerable<List<Employee>> GetEmployeesFromCloudApi(long startIndex, long limit)
        {
            string link = $"{endpoint.Value.OkdeskApi}/employees/list?api_token={okdeskSettings.Value.OkdeskApiToken}";

            await foreach (var employees in request.GetAllItems<Employee>(link, startIndex, limit))
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
            await foreach (List<Employee> employees in GetEmployeesFromCloudApi(startIndex, limit))
            {
                await unitOfWork.Employee.Upsert(employees, ct);

                await unitOfWork.SaveAsync(ct);
            }
        }

        public async Task UpdateEmployeesFromCloudDb(int startIndexEmployee, int limit, CancellationToken ct)
        {
            while (true)
            {
                List<Employee> employees = await GetEmployeesFromCloudDb(startIndexEmployee, limit);

                if (employees.Count == 0)
                    break;

                startIndexEmployee = employees.Last().Id;

                await unitOfWork.Employee.Upsert(employees, ct);

                await unitOfWork.SaveAsync(ct);
            }
        }
    }
}