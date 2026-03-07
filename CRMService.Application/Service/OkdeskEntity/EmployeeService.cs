using CRMService.Application.Abstractions.Database.Repository;
using CRMService.Application.Common.Mapping.OkdeskEntity;
using CRMService.Application.Models.ConfigClass;
using CRMService.Application.Service.Sync;
using CRMService.Contracts.Models.Dto.OkdeskEntity;
using CRMService.Contracts.Models.Responses.Results;
using CRMService.Domain.Models.Constants;
using CRMService.Domain.Models.OkdeskEntity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Runtime.CompilerServices;

namespace CRMService.Application.Service.OkdeskEntity
{
    public class EmployeeService(IOptions<ApiEndpointOptions> endpoint, IOptions<OkdeskOptions> okdeskSettings, IUnitOfWork unitOfWork, IOkdeskUnitOfWork okdeskUnitOfWork, IOkdeskEntityRequestService request, EntitySyncService sync, ILogger<EmployeeService> logger)
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

        private async Task<List<Employee>> GetEmployeesFromCloudDb(CancellationToken ct)
        {
            List<Employee> employees = await okdeskUnitOfWork.Employee.GetItemsByPredicateAsync(
                predicate: e => EF.Property<string>(e, "Type") == "Employee",
                asNoTracking: true,
                ct: ct);

            return employees.OrderBy(x => x.Id).ToList();
        }

        public async Task UpdateEmployeesFromCloudApi(CancellationToken ct)
        {
            logger.LogInformation("[Method:{MethodName}] Starting to update employees from API.", nameof(UpdateEmployeesFromCloudApi));

            await foreach (List<Employee> employees in GetEmployeesFromCloudApi(LimitConstants.LIMIT_FOR_RETRIEVING_ENTITIES_FROM_API, ct))
            {
                foreach (Employee employee in employees)
                {
                    await sync.RunExclusive(employee, async () =>
                    {
                        Employee? existingEmployee = await unitOfWork.Employee.GetItemByIdAsync(employee.Id, ct: ct);
                        if (existingEmployee == null)
                            unitOfWork.Employee.Create(employee);
                        else
                            existingEmployee.CopyData(employee);

                        await unitOfWork.SaveChangesAsync(ct);
                    }, ct);
                }
            }

            logger.LogInformation("[Method:{MethodName}] Update employees completed.", nameof(UpdateEmployeesFromCloudApi));
        }

        public async Task UpdateEmployeesFromCloudDb(CancellationToken ct)
        {
            logger.LogInformation("[Method:{MethodName}] Starting to update employees from DB.", nameof(UpdateEmployeesFromCloudDb));

            List<Employee> employees = await GetEmployeesFromCloudDb(ct);

            if (employees.Count != 0)
            {
                foreach (Employee employee in employees)
                {
                    await sync.RunExclusive(employee, async () =>
                    {
                        Employee? existingEmployee = await unitOfWork.Employee.GetItemByIdAsync(employee.Id, ct: ct);
                        if (existingEmployee == null)
                            unitOfWork.Employee.Create(employee);
                        else
                            existingEmployee.CopyData(employee);

                        await unitOfWork.SaveChangesAsync(ct);
                    }, ct);
                }
            }

            logger.LogInformation("[Method:{MethodName}] Update employees completed.", nameof(UpdateEmployeesFromCloudDb));
        }
    }
}