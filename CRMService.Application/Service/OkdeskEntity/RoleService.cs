using CRMService.Application.Abstractions.Database.Repository;
using CRMService.Application.Models.ConfigClass;
using CRMService.Domain.Models.Constants;
using CRMService.Domain.Models.OkdeskEntity;
using Microsoft.Extensions.Options;

namespace CRMService.Application.Service.OkdeskEntity
{
    public class RoleService(IOptions<ApiEndpointOptions> endpoint, IOptions<OkdeskOptions> okdeskSettings, EmployeeService employeeService, IOkdeskEntityRequestService request, IUnitOfWork unitOfWork, ILogger<RoleService> logger)
    {
        private async Task<List<OkdeskRole>> GetRolesFromCloudApi()
        {
            string link = $"{endpoint.Value.OkdeskApi}/employees/roles?api_token={okdeskSettings.Value.OkdeskApiToken}";

            return await request.GetRangeOfItemsAsync<OkdeskRole>(link);
        }

        public async Task UpdateRolesFromCloudApi(CancellationToken ct)
        {
            logger.LogInformation("[Method:{MethodName}] Starting to update roles from API.", nameof(UpdateRolesFromCloudApi));

            List<OkdeskRole> roles = await GetRolesFromCloudApi();

            if (roles.Count == 0)
                return;

            foreach (OkdeskRole role in roles)
            {
                OkdeskRole? existingRole = await unitOfWork.OkdeskRole.GetItemByIdAsync(role.Id, ct: ct);

                if (existingRole == null)
                    unitOfWork.OkdeskRole.Create(role);
                else
                    existingRole.CopyData(role);
            }

            await unitOfWork.SaveChangesAsync(ct);
        }

        public async Task UpsertEmployeeRoleConnectionsFromApi(CancellationToken ct)
        {
            logger.LogInformation("[Method:{MethodName}] Starting to update employee-role connections from API.", nameof(UpsertEmployeeRoleConnectionsFromApi));

            await foreach (List<Employee> employeesFromApi in employeeService.GetEmployeesFromCloudApi(LimitConstants.LIMIT_FOR_RETRIEVING_ENTITIES_FROM_API, ct))
            {
                // Собрать Id сотрудников и ролей из входных данных
                List<int> employeeIdsIncoming = employeesFromApi.Select(e => e.Id).Distinct().ToList();

                // Сформировать целевой набор связей сотрудник-роль
                List<EmployeeRole> desired = new();
                foreach (Employee employee in employeesFromApi)
                {
                    foreach (OkdeskRole role in employee.Roles)
                    {
                        desired.Add(new EmployeeRole
                        {
                            EmployeeId = employee.Id,
                            RoleId = role.Id
                        });
                    }
                }

                // Загрузить существующие связи по затронутым сотрудникам
                // Берём все роли этих сотрудников, чтобы удалить лишнее относительно снапшота из API
                List<EmployeeRole> existingLinks = await unitOfWork.EmployeeRole
                    .GetItemsByPredicateAsync(er => employeeIdsIncoming.Contains(er.EmployeeId), asNoTracking: true, ct: ct);

                List<EmployeeRole> toAdd = desired.Except(existingLinks, EmployeeRole.Comparer).ToList();
                List<EmployeeRole> toDelete = existingLinks.Except(desired, EmployeeRole.Comparer).ToList();

                if (toAdd.Count == 0 && toDelete.Count == 0)
                    continue;

                unitOfWork.EmployeeRole.CreateRange(toAdd);

                unitOfWork.EmployeeRole.DeleteRange(toDelete);

            }

            await unitOfWork.SaveChangesAsync(ct);
        }
    }
}