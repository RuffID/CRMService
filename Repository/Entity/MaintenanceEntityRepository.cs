using CRMService.Models.Entity;
using CRMService.Interfaces.Repository.Entity;
using System.Linq.Expressions;
using CRMService.Interfaces.Repository.Base;

namespace CRMService.Repository.Entity
{
    public class MaintenanceEntityRepository(IGetItemByIdRepository<MaintenanceEntity, int> _getById,
        ICreateItemRepository<MaintenanceEntity> _create,
        IUpsertItemByIdRepository<MaintenanceEntity, int> _upsert) : IMaintenanceEntityRepository
    {
        public Task<MaintenanceEntity?> GetItemById(int id, bool asNoTracking = false, CancellationToken ct = default, params Expression<Func<MaintenanceEntity, object>>[] includes)
            => _getById.GetItemById(id, asNoTracking, ct, includes);

        public Task<List<MaintenanceEntity>> GetItemsByPredicateAndSortById(Expression<Func<MaintenanceEntity, bool>>? predicate = null, int skip = 0, int? take = null, bool asNoTracking = false, CancellationToken ct = default, params Expression<Func<MaintenanceEntity, object>>[] includes)
            => _getById.GetItemsByPredicateAndSortById(predicate, skip, take, asNoTracking, ct, includes);

        public void Create(MaintenanceEntity item) => _create.Create(item);

        public Task Upsert(MaintenanceEntity item, CancellationToken ct = default)
            => _upsert.Upsert(item, ct);

        public Task Upsert(IEnumerable<MaintenanceEntity> items, CancellationToken ct = default)
            => _upsert.Upsert(items, ct);
    }
}
