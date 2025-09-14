using CRMService.Interfaces.Repository.Base;
using CRMService.Interfaces.Repository.Entity;
using CRMService.Models.Entity;
using System.Linq.Expressions;

namespace CRMService.DataBase.Repository.Entity
{
    public class IssueTypeGroupRepository(IGetItemByIdRepository<IssueTypeGroup, int> getItemById,
        IGetItemByPredicateRepository<IssueTypeGroup> getItemByPredicate,
        ICreateItemRepository<IssueTypeGroup> create,
        IUpsertItemByIdRepository<IssueTypeGroup, int> upsert) : IIssueTypeGroupRepository
    {
        public Task<IssueTypeGroup?> GetItemById(int id, bool asNoTracking = false, CancellationToken ct = default, params Expression<Func<IssueTypeGroup, object>>[] includes)
            => getItemById.GetItemById(id, asNoTracking, ct, includes);

        public Task<List<IssueTypeGroup>> GetItemsByPredicateAndSortById(Expression<Func<IssueTypeGroup, bool>>? predicate = null, int skip = 0, int? take = null, bool asNoTracking = false, CancellationToken ct = default, params Expression<Func<IssueTypeGroup, object>>[] includes)
        => getItemById.GetItemsByPredicateAndSortById(predicate, skip, take, asNoTracking, ct, includes);

        public Task<IssueTypeGroup?> GetItemByPredicate(Expression<Func<IssueTypeGroup, bool>> predicate, bool asNoTracking = false, CancellationToken ct = default, params Expression<Func<IssueTypeGroup, object>>[] includes)
            => getItemByPredicate.GetItemByPredicate(predicate, asNoTracking, ct, includes);

        public Task<List<IssueTypeGroup>> GetItemsByPredicate(Expression<Func<IssueTypeGroup, bool>>? predicate = null, int skip = 0, int? take = null, bool asNoTracking = false, CancellationToken ct = default, params Expression<Func<IssueTypeGroup, object>>[] includes)
            => getItemByPredicate.GetItemsByPredicate(predicate, skip, take, asNoTracking, ct, includes);

        public void Create(IssueTypeGroup item) => create.Create(item);

        public Task Upsert(IssueTypeGroup item, CancellationToken ct = default)
            => upsert.Upsert(item, ct);

        public Task Upsert(IEnumerable<IssueTypeGroup> items, CancellationToken ct = default)
            => upsert.Upsert(items, ct);
    }
}
