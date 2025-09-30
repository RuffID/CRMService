using CRMService.Interfaces.Repository.Base;
using CRMService.Interfaces.Repository.OkdeskEntity;
using CRMService.Models.OkdeskEntity;
using System.Linq.Expressions;

namespace CRMService.DataBase.Repository.Entity
{
    public class OkdeskRoleRepository(IGetItemByIdRepository<OkdeskRole, int> getItemByid,
        IGetItemByPredicateRepository<OkdeskRole> getItemByPredicate,
        ICreateItemRepository<OkdeskRole> create,
        IUpsertItemByIdRepository<OkdeskRole, int> upsert) : IOkdeskRoleRepository
    {
        public Task<OkdeskRole?> GetItemById(int id, bool asNoTracking = false, CancellationToken ct = default, params Expression<Func<OkdeskRole, object>>[] includes)
            => getItemByid.GetItemById(id, asNoTracking, ct, includes);

        public Task<OkdeskRole?> GetItemByPredicate(Expression<Func<OkdeskRole, bool>> predicate, bool asNoTracking = false, CancellationToken ct = default, params Expression<Func<OkdeskRole, object>>[] includes)
            => getItemByPredicate.GetItemByPredicate(predicate, asNoTracking, ct, includes);

        public Task<List<OkdeskRole>> GetItemsByPredicate(Expression<Func<OkdeskRole, bool>>? predicate = null, int skip = 0, int? take = null, bool asNoTracking = false, CancellationToken ct = default, params Expression<Func<OkdeskRole, object>>[] includes)
            => getItemByPredicate.GetItemsByPredicate(predicate, skip, take, asNoTracking, ct, includes);

        public Task<List<OkdeskRole>> GetItemsByPredicateAndSortById(Expression<Func<OkdeskRole, bool>>? predicate = null, int skip = 0, int? take = null, bool asNoTracking = false, CancellationToken ct = default, params Expression<Func<OkdeskRole, object>>[] includes)
            => getItemByid.GetItemsByPredicateAndSortById(predicate, skip, take, asNoTracking, ct, includes);

        public void Create(OkdeskRole item) => create.Create(item);

        public Task Upsert(OkdeskRole item, CancellationToken ct = default)
            => upsert.Upsert(item, ct);

        public Task Upsert(IEnumerable<OkdeskRole> items, CancellationToken ct = default)
            => upsert.Upsert(items, ct);
    }
}
