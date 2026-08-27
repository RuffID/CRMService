using CRMService.Application.Abstractions.Database.Repository.Base;
using CRMService.Domain.Models.OkdeskEntity;

namespace CRMService.Application.Abstractions.Database.Repository.Entity
{
    public interface IEmployeeRepository :
        IGetItemByIdRepository<Employee, int>,
        IGetItemsRepository<Employee>,
        ICreateItemRepository<Employee>
    {
        Task<List<Employee>> GetWithGroupsReadOnlyAsync(IReadOnlyCollection<int>? groupIds, bool includeInactive, CancellationToken ct = default);
        Task<List<Employee>> SearchReadOnlyAsync(string? search, bool activeOnly, CancellationToken ct = default);
        Task<List<Employee>> GetByIdsReadOnlyAsync(IReadOnlyCollection<int> ids, bool activeOnly = false, CancellationToken ct = default);
        Task<List<Employee>> GetActiveFromIdReadOnlyAsync(long startId, bool inclusive, CancellationToken ct = default);
    }
}
