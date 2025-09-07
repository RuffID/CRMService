using CRMService.Models.Entity;
using CRMService.Interfaces.Repository.Entity;
using System.Linq.Expressions;
using CRMService.Interfaces.Repository.Base;

namespace CRMService.Repository.Entity
{
    public class MaintenanceEntityRepository(IGetItemByIdRepository<MaintenanceEntity, int> getItemById,
        ICreateItemRepository<MaintenanceEntity> create,
        IUpsertItemByIdRepository<MaintenanceEntity, int> upsert) : IMaintenanceEntityRepository
    {
        public Task<MaintenanceEntity?> GetItemById(int id, bool asNoTracking = false, CancellationToken ct = default, params Expression<Func<MaintenanceEntity, object>>[] includes)
            => getItemById.GetItemById(id, asNoTracking, ct, includes);

        public Task<List<MaintenanceEntity>> GetItemsByPredicateAndSortById(Expression<Func<MaintenanceEntity, bool>>? predicate = null, int skip = 0, int? take = null, bool asNoTracking = false, CancellationToken ct = default, params Expression<Func<MaintenanceEntity, object>>[] includes)
            => getItemById.GetItemsByPredicateAndSortById(predicate, skip, take, asNoTracking, ct, includes);

        public void Create(MaintenanceEntity item) => create.Create(item);

        public Task Upsert(MaintenanceEntity item, CancellationToken ct = default)
            => upsert.Upsert(item, ct);

        public Task Upsert(IEnumerable<MaintenanceEntity> items, CancellationToken ct = default)
            => upsert.Upsert(items, ct);
    }
}
