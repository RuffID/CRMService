using CRMService.Application.Abstractions.Database.Repository.Base;
using CRMService.Domain.Models.OkdeskEntity;

namespace CRMService.Application.Abstractions.Database.Repository.Entity
{
    public interface IIssuePriorityRepository :
        IGetItemByIdRepository<IssuePriority, int>,
        IGetItemsRepository<IssuePriority>,
        ICreateItemRepository<IssuePriority>
    {
        Task<IssuePriority?> GetByCodeAsync(string code, CancellationToken ct = default);
        Task<IssuePriority?> GetByCodeReadOnlyAsync(string code, CancellationToken ct = default);
        Task<List<IssuePriority>> SearchReadOnlyAsync(string? search, CancellationToken ct = default);
        Task<List<IssuePriority>> GetByCodesReadOnlyAsync(IReadOnlyCollection<string> codes, CancellationToken ct = default);
    }
}
