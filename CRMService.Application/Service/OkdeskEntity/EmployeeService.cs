using CRMService.Application.Abstractions.Database.Repository;
using CRMService.Application.Models.ConfigClass;
using CRMService.Domain.Models.Constants;
using CRMService.Contracts.Models.Dto.OkdeskEntity;
using CRMService.Domain.Models.OkdeskEntity;
using CRMService.Contracts.Models.Responses.Results;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Data;
using System.Runtime.CompilerServices;
using CRMService.Application.Common.Mapping.OkdeskEntity;

namespace CRMService.Application.Service.OkdeskEntity
{
    public class EmployeeService(IOptions<ApiEndpointOptions> endpoint, IOptions<OkdeskOptions> okdeskSettings, IUnitOfWork unitOfWork, postgresSelect postgresSelect, IOkdeskEntityRequestService request, ILogger<EmployeeService> logger)
    {
        public async Task<ServiceResult<List<EmployeeDto>>> GetEmployees(List<int>? groupIds = null, CancellationToken ct = default)
        {
            List<Employee> employees;
            if (groupIds != null && groupIds.Count > 0)
            {
                employees = await unitOfWork.Employee.GetItemsByPredicateAsync(predicate: e => e.EmployeeGroups.Any(eg => groupIds.Contains(eg.GroupId)) && e.Active,
                    asNoTracking: true,
                    include: e => e.Include(x => x.EmployeeGroups).ThenInclude(eg => eg.Group),
                    ct: ct);
            }
            else
            {
                employees = await unitOfWork.Employee.GetItemsByPredicateAsync(predicate: e => e.Active, 
                    asNoTracking: true,
                    include: e => e.Include(x => x.EmployeeGroups).ThenInclude(eg => eg.Group),
                    ct: ct);
            }

            return ServiceResult<List<EmployeeDto>>.Ok(employees.ToDto().ToList());
        }

        public async IAsyncEnumerable<List<Employee>> GetEmployeesFromCloudApi(long limit, [EnumeratorCancellation] CancellationToken ct)
        {
            string link = $"{endpoint.Value.OkdeskApi}/employees/list?api_token={okdeskSettings.Value.OkdeskApiToken}";

            await foreach (List<Employee> employees in request.GetAllItemsAsync<Employee>(link, startIndex: 0, limit, ct: ct))
                yield return employees;
        }

        private async Task<List<Employee>> GetEmployeesFromCloudDb(int limit, int lastSequentialId, CancellationToken ct)
        {
            string sqlCommand = string.Format("SELECT * FROM users " +
                "WHERE type = 'Employee' " +
                "AND users.sequential_id > {0} " +
                "ORDER BY users.sequential_id " +
                "LIMIT {1};", lastSequentialId, limit);

            DataSet ds = await postgresSelect.Select(sqlCommand, ct);
            DataTable? employeesTable = ds.Tables["Table"];
            if (employeesTable == null)
                return new();

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

        public async Task UpdateEmployeesFromCloudApi(CancellationToken ct)
        {
            logger.LogInformation("[Method:{MethodName}] Starting to update employees from API.", nameof(UpdateEmployeesFromCloudApi));

            await foreach (List<Employee> employees in GetEmployeesFromCloudApi(LimitConstants.LIMIT_FOR_RETRIEVING_ENTITIES_FROM_API, ct))
            {
                foreach (Employee employee in employees)
                {
                    Employee? existingEmployee = await unitOfWork.Employee.GetItemByIdAsync(employee.Id, ct: ct);
                    if (existingEmployee == null)
                        unitOfWork.Employee.Create(employee);
                    else
                        existingEmployee.CopyData(employee);
                }
            }

            await unitOfWork.SaveChangesAsync(ct);
        }

        public async Task UpdateEmployeesFromCloudDb(CancellationToken ct)
        {
            logger.LogInformation("[Method:{MethodName}] Starting to update employees from DB.", nameof(UpdateEmployeesFromCloudDb));

            int lastSequentialId = 0;
            while (true)
            {
                List<Employee> employees = await GetEmployeesFromCloudDb(LimitConstants.LIMIT_FOR_RETRIEVING_ENTITIES_FROM_DB, lastSequentialId, ct);

                if (employees.Count == 0)
                    break;

                foreach (Employee employee in employees)
                {
                    Employee? existingEmployee = await unitOfWork.Employee.GetItemByIdAsync(employee.Id, ct: ct);
                    if (existingEmployee == null)
                        unitOfWork.Employee.Create(employee);
                    else
                        existingEmployee.CopyData(employee);
                }

                lastSequentialId = employees.Last().Id;
            }

            await unitOfWork.SaveChangesAsync(ct);
        }
    }
}
