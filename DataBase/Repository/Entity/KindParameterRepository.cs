using CRMService.Interfaces.Repository.Base;
using CRMService.Interfaces.Repository.OkdeskEntity;
using CRMService.Models.OkdeskEntity;
using System.Linq.Expressions;

namespace CRMService.DataBase.Repository.Entity
{
    public class KindParameterRepository(IGetItemByIdRepository<KindsParameter, int> getItemById,
        IGetItemByPredicateRepository<KindsParameter> getItemByPredicate,
        ICreateItemRepository<KindsParameter> create,
        IUpsertItemByPredicateRepository<KindsParameter> upsertByPredicate,
        IUpsertItemByIdRepository<KindsParameter, int> upsertById) : IKindParameterRepository
    {
        public Task<KindsParameter?> GetItemById(int id, bool asNoTracking = false, CancellationToken ct = default, params Expression<Func<KindsParameter, object>>[] includes)
            => getItemById.GetItemById(id, asNoTracking, ct, includes);

        public Task<List<KindsParameter>> GetItemsByPredicateAndSortById(Expression<Func<KindsParameter, bool>>? predicate = null, int skip = 0, int? take = null, bool asNoTracking = false, CancellationToken ct = default, params Expression<Func<KindsParameter, object>>[] includes)
            => getItemById.GetItemsByPredicateAndSortById(predicate, skip, take, asNoTracking, ct, includes);

        public Task<KindsParameter?> GetItemByPredicate(Expression<Func<KindsParameter, bool>> predicate, bool asNoTracking = false, CancellationToken ct = default, params Expression<Func<KindsParameter, object>>[] includes)
            => getItemByPredicate.GetItemByPredicate(predicate, asNoTracking, ct, includes);

        public Task<List<KindsParameter>> GetItemsByPredicate(Expression<Func<KindsParameter, bool>>? predicate = null, int skip = 0, int? take = null, bool asNoTracking = false, CancellationToken ct = default, params Expression<Func<KindsParameter, object>>[] includes)
            => getItemByPredicate.GetItemsByPredicate(predicate, skip, take, asNoTracking, ct, includes);

        public void Create(KindsParameter item) => create.Create(item);

        public Task Upsert(KindsParameter item, Expression<Func<KindsParameter, bool>> predicate, CancellationToken ct = default)
            => upsertByPredicate.Upsert(item, predicate, ct);

        public Task Upsert(IEnumerable<KindsParameter> items, Func<KindsParameter, Expression<Func<KindsParameter, bool>>> predicateFactory, CancellationToken ct = default)
            => upsertByPredicate.Upsert(items, predicateFactory, ct);

        public Task Upsert(KindsParameter item, CancellationToken ct = default) => upsertById.Upsert(item, ct);

        public Task Upsert(IEnumerable<KindsParameter> items, CancellationToken ct = default) => upsertById.Upsert(items, ct);
    }
}