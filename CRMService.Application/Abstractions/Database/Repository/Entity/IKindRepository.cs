using CRMService.Application.Abstractions.Database.Repository.Base;
using CRMService.Domain.Models.OkdeskEntity;

namespace CRMService.Application.Abstractions.Database.Repository.Entity
{
    public interface IKindRepository :
        IGetItemByIdRepository<Kind, int>,
        IGetItemsRepository<Kind>,
        ICreateItemRepository<Kind>
    {
        Task<Kind?> GetByCodeReadOnlyAsync(string code, CancellationToken ct = default);
        Task<List<Kind>> SearchReadOnlyAsync(string? search, IReadOnlyCollection<int>? modelIds, CancellationToken ct = default);
        Task<List<Kind>> GetByIdsReadOnlyAsync(IReadOnlyCollection<int> ids, CancellationToken ct = default);
        Task<List<Kind>> GetByCodesReadOnlyAsync(IReadOnlyCollection<string> codes, CancellationToken ct = default);
    }
}
