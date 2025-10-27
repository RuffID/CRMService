using CRMService.Interfaces.Repository.Authorization;
using CRMService.Interfaces.Repository.Base;
using CRMService.Models.Authorization;
using System.Linq.Expressions;

namespace CRMService.DataBase.Repository.Authorization
{
    public class SessionRepository(IGetItemByIdRepository<Session, Guid> getItemById,
        IGetItemByPredicateRepository<Session> getItemByPredicate,
        ICreateItemRepository<Session> create,
        IUpsertItemByIdRepository<Session, Guid> upsert,
        IDeleteItemRepository<Session> delete) : ISessionRepository
    {
        public Task<Session?> GetItemById(Guid id, bool asNoTracking = false, CancellationToken ct = default, params Expression<Func<Session, object>>[] includes)
            => getItemById.GetItemById(id, asNoTracking, ct, includes);
                

        public Task<List<Session>> GetItemsByPredicateAndSortById(Expression<Func<Session, bool>>? predicate = null, int skip = 0, int? take = null, bool asNoTracking = false, CancellationToken ct = default, params Expression<Func<Session, object>>[] includes)
            => getItemById.GetItemsByPredicateAndSortById(predicate, skip, take, asNoTracking, ct, includes);

        public Task<Session?> GetItemByPredicate(Expression<Func<Session, bool>> predicate, bool asNoTracking = false, CancellationToken ct = default, params Expression<Func<Session, object>>[] includes)
            => getItemByPredicate.GetItemByPredicate(predicate, asNoTracking, ct, includes);

        public Task<List<Session>> GetItemsByPredicate(Expression<Func<Session, bool>>? predicate = null, int skip = 0, int? take = null, bool asNoTracking = false, CancellationToken ct = default, params Expression<Func<Session, object>>[] includes)
            => getItemByPredicate.GetItemsByPredicate(predicate, skip, take, asNoTracking, ct, includes);

        public void Create(Session item) => create.Create(item);

        public Task Upsert(Session item, CancellationToken ct = default) => upsert.Upsert(item, ct);

        public Task Upsert(IEnumerable<Session> items, CancellationToken ct = default) => upsert.Upsert(items, ct);

        public void Delete(Session item) => delete.Delete(item);

        public void DeleteRange(IEnumerable<Session> entities) => delete.DeleteRange(entities);
    }
}
