using CRMService.Models.OkdeskEntity;
using CRMService.Interfaces.Repository.OkdeskEntity;
using System.Linq.Expressions;
using CRMService.Interfaces.Repository.Base;

namespace CRMService.DataBase.Repository.Entity
{
    public class KindRepository(IGetItemByIdRepository<Kind, int> getItemById,
        IGetItemByPredicateRepository<Kind> getItemByPredicate,
        ICreateItemRepository<Kind> create,
        IUpsertItemByIdRepository<Kind, int> upsert) : IKindRepository
    {
        public Task<Kind?> GetItemByPredicate(Expression<Func<Kind, bool>> predicate, bool asNoTracking = false, CancellationToken ct = default, params Expression<Func<Kind, object>>[] includes)
            => getItemByPredicate.GetItemByPredicate(predicate, asNoTracking, ct, includes);

        public Task<List<Kind>> GetItemsByPredicate(Expression<Func<Kind, bool>>? predicate = null, int skip = 0, int? take = null, bool asNoTracking = false, CancellationToken ct = default, params Expression<Func<Kind, object>>[] includes)
            => getItemByPredicate.GetItemsByPredicate(predicate, skip, take, asNoTracking, ct, includes);

        public Task<Kind?> GetItemById(int id, bool asNoTracking = false, CancellationToken ct = default, params Expression<Func<Kind, object>>[] includes)
            => getItemById.GetItemById(id, asNoTracking, ct, includes);

        public Task<List<Kind>> GetItemsByPredicateAndSortById(Expression<Func<Kind, bool>>? predicate = null, int skip = 0, int? take = null, bool asNoTracking = false, CancellationToken ct = default, params Expression<Func<Kind, object>>[] includes)
            => getItemById.GetItemsByPredicateAndSortById(predicate, skip, take, asNoTracking, ct, includes);

        public void Create(Kind item) => create.Create(item);

        public Task Upsert(Kind item, CancellationToken ct = default) => upsert.Upsert(item, ct);

        public Task Upsert(IEnumerable<Kind> items, CancellationToken ct = default) => upsert.Upsert(items, ct);
    }
}
