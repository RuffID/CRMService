using CRMService.Application.Abstractions.Database.Repository;

namespace CRMService.Application.Service.OkdeskEntity.Resolvers
{
    public class EmployeeResolverService(
        IUnitOfWork unitOfWork,
        EmployeeService employeeService,
        ReferenceResolveHelper referenceResolveHelper,
        ILogger<EmployeeResolverService> logger)
    {
        public Task<int?> ResolveEmployeeIdAsync(int employeeId, int issueId, CancellationToken ct)
        {
            return referenceResolveHelper.ResolveAsync(
                employeeId,
                async token => (await unitOfWork.Employee.GetItemByIdAsync(employeeId, true, ct: token))?.Id,
                employeeService.UpdateEmployeesFromCloudApi,
                id => $"employee:{id}",
                id => $"Employee with id: {id} was not found for issue with id: {issueId}. Refreshing employees from API.",
                id => $"Employee with id '{id}' was not found after refresh for issue '{issueId}'.",
                logger,
                nameof(ResolveEmployeeIdAsync),
                ct);
        }
    }
}