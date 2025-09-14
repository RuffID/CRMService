using CRMService.Interfaces.Repository.Base;
using CRMService.Interfaces.Repository.Entity;
using CRMService.Models.Entity;
using System.Linq.Expressions;

namespace CRMService.DataBase.Repository.Entity
{
    public class IssueStatusRepository(IGetItemByIdRepository<IssueStatus, int> getItemById,
        IGetItemByPredicateRepository<IssueStatus> getItemByPredicate,
        ICreateItemRepository<IssueStatus> create,
        IUpsertItemByIdRepository<IssueStatus, int> upsertItemById) : IIssueStatusRepository
    {
        public Task<IssueStatus?> GetItemById(int id, bool asNoTracking = false, CancellationToken ct = default, params Expression<Func<IssueStatus, object>>[] includes)
            => getItemById.GetItemById(id, asNoTracking, ct, includes);

        public Task<List<IssueStatus>> GetItemsByPredicateAndSortById(Expression<Func<IssueStatus, bool>>? predicate = null, int skip = 0, int? take = null, bool asNoTracking = false, CancellationToken ct = default, params Expression<Func<IssueStatus, object>>[] includes)
            => getItemById.GetItemsByPredicateAndSortById(predicate, skip, take, asNoTracking, ct, includes);

        public Task<IssueStatus?> GetItemByPredicate(Expression<Func<IssueStatus, bool>> predicate, bool asNoTracking = false, CancellationToken ct = default, params Expression<Func<IssueStatus, object>>[] includes)
            => getItemByPredicate.GetItemByPredicate(predicate, asNoTracking, ct, includes);

        public Task<List<IssueStatus>> GetItemsByPredicate(Expression<Func<IssueStatus, bool>>? predicate = null, int skip = 0, int? take = null, bool asNoTracking = false, CancellationToken ct = default, params Expression<Func<IssueStatus, object>>[] includes)
            => getItemByPredicate.GetItemsByPredicate(predicate, skip, take, asNoTracking, ct, includes);

        public void Create(IssueStatus item) => create.Create(item);

        public Task Upsert(IssueStatus item, CancellationToken ct = default) => upsertItemById.Upsert(item, ct);

        public Task Upsert(IEnumerable<IssueStatus> items, CancellationToken ct = default) => upsertItemById.Upsert(items, ct);
    }
}
