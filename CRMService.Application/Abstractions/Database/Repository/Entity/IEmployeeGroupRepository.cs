using CRMService.Application.Abstractions.Database.Repository.Base;
using CRMService.Domain.Models.OkdeskEntity;

namespace CRMService.Application.Abstractions.Database.Repository.Entity
{
    public interface IEmployeeGroupRepository :
        IGetItemsRepository<EmployeeGroup>,
        ICreateItemRepository<EmployeeGroup>,
        IDeleteItemRepository<EmployeeGroup>
    {
        Task<List<EmployeeGroup>> GetByGroupIdsReadOnlyAsync(IReadOnlyCollection<int> groupIds, CancellationToken ct = default);
        Task<List<EmployeeGroup>> GetByEmployeesAndGroupsReadOnlyAsync(IReadOnlyCollection<int> employeeIds, IReadOnlyCollection<int> groupIds, CancellationToken ct = default);
    }
}
