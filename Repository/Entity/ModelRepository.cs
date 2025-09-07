using CRMService.Interfaces.Repository.Base;
using CRMService.Interfaces.Repository.Entity;
using CRMService.Models.Entity;
using System.Linq.Expressions;

namespace CRMService.Repository.Entity
{
    public class ModelRepository(IGetItemByIdRepository<Model, int> getItemById,
        IGetItemByPredicateRepository<Model> getItemByPredicate,
        ICreateItemRepository<Model> create,
        IUpsertItemByCodeRepository<Model> upsert) : IModelRepository
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

        public Task UpsertByCode(Model item, CancellationToken ct = default)
            => upsert.UpsertByCode(item, ct);

        public Task UpsertByCode(string oldCode, Model item, CancellationToken ct = default)
            => upsert.UpsertByCode(oldCode, item, ct);

        public Task UpsertByCodePairs(IEnumerable<(string OldCode, Model Item)> items, CancellationToken ct = default)
            => upsert.UpsertByCodePairs(items, ct);

        public Task UpsertByCodes(IEnumerable<Model> items, CancellationToken ct = default)
            => upsert.UpsertByCodes(items, ct);        
    }
}
