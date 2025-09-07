using CRMService.Interfaces.Repository.Base;
using CRMService.Interfaces.Repository.Entity;
using CRMService.Models.Entity;
using System.Linq.Expressions;

namespace CRMService.Repository.Entity
{
    public class IssueTypeRepository(IGetItemByIdRepository<IssueType, int> getItemById,
        IGetItemByPredicateRepository<IssueType> getItemByPredicate,
        ICreateItemRepository<IssueType> create,
        IUpsertItemByCodeRepository<IssueType> upsert) : IIssueTypeRepository
    {
        public Task<IssueType?> GetItemById(int id, bool asNoTracking = false, CancellationToken ct = default, params Expression<Func<IssueType, object>>[] includes)
            => getItemById.GetItemById(id, asNoTracking, ct, includes);

        public Task<List<IssueType>> GetItemsByPredicateAndSortById(Expression<Func<IssueType, bool>>? predicate = null, int skip = 0, int? take = null, bool asNoTracking = false, CancellationToken ct = default, params Expression<Func<IssueType, object>>[] includes)
        => getItemById.GetItemsByPredicateAndSortById(predicate, skip, take, asNoTracking, ct, includes);

        public Task<IssueType?> GetItemByPredicate(Expression<Func<IssueType, bool>> predicate, bool asNoTracking = false, CancellationToken ct = default, params Expression<Func<IssueType, object>>[] includes)
            => getItemByPredicate.GetItemByPredicate(predicate, asNoTracking, ct, includes);

        public Task<List<IssueType>> GetItemsByPredicate(Expression<Func<IssueType, bool>>? predicate = null, int skip = 0, int? take = null, bool asNoTracking = false, CancellationToken ct = default, params Expression<Func<IssueType, object>>[] includes)
            => getItemByPredicate.GetItemsByPredicate(predicate, skip, take, asNoTracking, ct, includes);

        public void Create(IssueType item) => create.Create(item);

        public Task UpsertByCode(IssueType item, CancellationToken ct = default) => upsert.UpsertByCode(item, ct);

        public Task UpsertByCode(string oldCode, IssueType item, CancellationToken ct = default) => upsert.UpsertByCode(oldCode, item, ct);

        public Task UpsertByCodePairs(IEnumerable<(string OldCode, IssueType Item)> items, CancellationToken ct = default)
            => upsert.UpsertByCodePairs(items, ct);

        public Task UpsertByCodes(IEnumerable<IssueType> items, CancellationToken ct = default) => upsert.UpsertByCodes(items, ct);        
    }
}
