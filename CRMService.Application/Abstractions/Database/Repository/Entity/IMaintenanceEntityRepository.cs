using CRMService.Application.Abstractions.Database.Repository.Base;
using CRMService.Domain.Models.OkdeskEntity;

namespace CRMService.Application.Abstractions.Database.Repository.Entity
{
    public interface IMaintenanceEntityRepository :
        IGetItemByIdRepository<MaintenanceEntity, int>,
        IGetItemsRepository<MaintenanceEntity>,
        ICreateItemRepository<MaintenanceEntity>
    {
        Task<List<MaintenanceEntity>> SearchReadOnlyAsync(string? search, IReadOnlyCollection<int>? companyIds, CancellationToken ct = default);
        Task<List<MaintenanceEntity>> GetByIdsReadOnlyAsync(IReadOnlyCollection<int> ids, CancellationToken ct = default);
    }
}
