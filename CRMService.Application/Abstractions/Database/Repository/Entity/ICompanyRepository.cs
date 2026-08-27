using CRMService.Application.Abstractions.Database.Repository.Base;
using CRMService.Domain.Models.OkdeskEntity;

namespace CRMService.Application.Abstractions.Database.Repository.Entity
{
    public interface ICompanyRepository :
        IGetItemByIdRepository<Company, int>,
        IGetItemsRepository<Company>,
        ICreateItemRepository<Company>
    {
        Task<List<Company>> GetByCategoryCodeReadOnlyAsync(string categoryCode, CancellationToken ct = default);
        Task<List<Company>> SearchReadOnlyAsync(string? search, CancellationToken ct = default);
        Task<List<Company>> GetByIdsReadOnlyAsync(IReadOnlyCollection<int> ids, CancellationToken ct = default);
    }
}
