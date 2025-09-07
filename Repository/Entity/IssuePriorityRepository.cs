using CRMService.Interfaces.Repository.Base;
using CRMService.Interfaces.Repository.Entity;
using CRMService.Models.Entity;
using System.Linq.Expressions;

namespace CRMService.Repository.Entity
{
    public class IssuePriorityRepository(IGetItemByIdRepository<IssuePriority, int> getItemById,
        IGetItemByPredicateRepository<IssuePriority> getItemByPredicate,
        ICreateItemRepository<IssuePriority> createItem,
        IUpsertItemByCodeRepository<IssuePriority> upsertItemByCode) : IIssuePriorityRepository
    {
        public Task<IssuePriority?> GetItemById(int id, bool asNoTracking = false, CancellationToken ct = default, params Expression<Func<IssuePriority, object>>[] includes)
            => getItemById.GetItemById(id, asNoTracking, ct, includes);

        public Task<List<IssuePriority>> GetItemsByPredicateAndSortById(Expression<Func<IssuePriority, bool>>? predicate = null, int skip = 0, int? take = null, bool asNoTracking = false, CancellationToken ct = default, params Expression<Func<IssuePriority, object>>[] includes)
            => getItemById.GetItemsByPredicateAndSortById(predicate, skip, take, asNoTracking, ct, includes);

        public Task<IssuePriority?> GetItemByPredicate(Expression<Func<IssuePriority, bool>> predicate, bool asNoTracking = false, CancellationToken ct = default, params Expression<Func<IssuePriority, object>>[] includes)
            => getItemByPredicate.GetItemByPredicate(predicate, asNoTracking, ct, includes);

        public Task<List<IssuePriority>> GetItemsByPredicate(Expression<Func<IssuePriority, bool>>? predicate = null, int skip = 0, int? take = null, bool asNoTracking = false, CancellationToken ct = default, params Expression<Func<IssuePriority, object>>[] includes)
            => getItemByPredicate.GetItemsByPredicate(predicate, skip, take, asNoTracking, ct, includes);

        public void Create(IssuePriority item) => createItem.Create(item);

        public Task UpsertByCode(IssuePriority item, CancellationToken ct = default) => upsertItemByCode.UpsertByCode(item, ct);

        public Task UpsertByCode(string oldCode, IssuePriority item, CancellationToken ct = default)
            => upsertItemByCode.UpsertByCode(oldCode, item, ct);

        public Task UpsertByCodePairs(IEnumerable<(string OldCode, IssuePriority Item)> items, CancellationToken ct = default)
            => upsertItemByCode.UpsertByCodePairs(items, ct);

        public Task UpsertByCodes(IEnumerable<IssuePriority> items, CancellationToken ct = default) => upsertItemByCode.UpsertByCodes(items, ct);        
    }
}
