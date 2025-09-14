using CRMService.Interfaces.Repository.Base;
using CRMService.Interfaces.Repository.Entity;
using CRMService.Models.Entity;
using System.Linq.Expressions;

namespace CRMService.DataBase.Repository.Entity
{
    public class ModelRepository(IGetItemByIdRepository<Model, int> getItemById,
        IUpsertItemByIdRepository<Model, int> upsertItemById,
        IGetItemByPredicateRepository<Model> getItemByPredicate,
        ICreateItemRepository<Model> create) : IModelRepository
    {
        public Task<Model?> GetItemByPredicate(Expression<Func<Model, bool>> predicate, bool asNoTracking = false, CancellationToken ct = default, params Expression<Func<Model, object>>[] includes)
            => getItemByPredicate.GetItemByPredicate(predicate, asNoTracking, ct, includes);

        public Task<List<Model>> GetItemsByPredicate(Expression<Func<Model, bool>>? predicate = null, int skip = 0, int? take = null, bool asNoTracking = false, CancellationToken ct = default, params Expression<Func<Model, object>>[] includes)
            => getItemByPredicate.GetItemsByPredicate(predicate, skip, take, asNoTracking, ct, includes);

        public Task<Model?> GetItemById(int id, bool asNoTracking = false, CancellationToken ct = default, params Expression<Func<Model, object>>[] includes)
            => getItemById.GetItemById(id, asNoTracking, ct, includes);

        public Task<List<Model>> GetItemsByPredicateAndSortById(Expression<Func<Model, bool>>? predicate = null, int skip = 0, int? take = null, bool asNoTracking = false, CancellationToken ct = default, params Expression<Func<Model, object>>[] includes)
            => getItemById.GetItemsByPredicateAndSortById(predicate, skip, take, asNoTracking, ct, includes);

        public void Create(Model item) => create.Create(item);

        public Task Upsert(Model item, CancellationToken ct = default)
            => upsertItemById.Upsert(item, ct);

        public Task Upsert(IEnumerable<Model> items, CancellationToken ct = default)
            => upsertItemById.Upsert(items, ct);
    }
}
