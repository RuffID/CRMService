using CRMService.Models.OkdeskEntity;
using CRMService.Interfaces.Repository.OkdeskEntity;
using CRMService.Interfaces.Repository.Base;
using System.Linq.Expressions;

namespace CRMService.DataBase.Repository.Entity
{
    public class ManufacturerRepository(IGetItemByIdRepository<Manufacturer, int> getItemById,
        IGetItemByPredicateRepository<Manufacturer> getItemByPredicate,
        IUpsertItemByIdRepository<Manufacturer, int> upsertById,
        ICreateItemRepository<Manufacturer> create) : IManufacturerRepository
    {
        public Task<Manufacturer?> GetItemByPredicate(Expression<Func<Manufacturer, bool>> predicate, bool asNoTracking = false, CancellationToken ct = default, params Expression<Func<Manufacturer, object>>[] includes)
            => getItemByPredicate.GetItemByPredicate(predicate, asNoTracking, ct, includes);

        public Task<List<Manufacturer>> GetItemsByPredicate(Expression<Func<Manufacturer, bool>>? predicate = null, int skip = 0, int? take = null, bool asNoTracking = false, CancellationToken ct = default, params Expression<Func<Manufacturer, object>>[] includes)
            => getItemByPredicate.GetItemsByPredicate(predicate, skip, take, asNoTracking, ct, includes);

        public Task<Manufacturer?> GetItemById(int id, bool asNoTracking = false, CancellationToken ct = default, params Expression<Func<Manufacturer, object>>[] includes)
            => getItemById.GetItemById(id, asNoTracking, ct, includes);

        public Task<List<Manufacturer>> GetItemsByPredicateAndSortById(Expression<Func<Manufacturer, bool>>? predicate = null, int skip = 0, int? take = null, bool asNoTracking = false, CancellationToken ct = default, params Expression<Func<Manufacturer, object>>[] includes)
            => getItemById.GetItemsByPredicateAndSortById(predicate, skip, take, asNoTracking, ct, includes);

        public void Create(Manufacturer item) => create.Create(item);

        public Task Upsert(Manufacturer item, CancellationToken ct = default) => upsertById.Upsert(item, ct);

        public Task Upsert(IEnumerable<Manufacturer> items, CancellationToken ct = default) => upsertById.Upsert(items, ct);
    }
}
