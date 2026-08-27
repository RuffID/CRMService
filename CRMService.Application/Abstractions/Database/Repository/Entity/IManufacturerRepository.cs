using CRMService.Application.Abstractions.Database.Repository.Base;
using CRMService.Domain.Models.OkdeskEntity;

namespace CRMService.Application.Abstractions.Database.Repository.Entity
{
    public interface IManufacturerRepository :
        IGetItemByIdRepository<Manufacturer, int>,
        IGetItemsRepository<Manufacturer>,
        ICreateItemRepository<Manufacturer>
    {
        Task<Manufacturer?> GetByCodeReadOnlyAsync(string code, CancellationToken ct = default);
        Task<List<Manufacturer>> SearchReadOnlyAsync(string? search, IReadOnlyCollection<int>? modelIds, CancellationToken ct = default);
        Task<List<Manufacturer>> GetByCodesReadOnlyAsync(IReadOnlyCollection<string> codes, CancellationToken ct = default);
    }
}
