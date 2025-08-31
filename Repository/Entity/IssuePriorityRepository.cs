using CRMService.Interfaces.Repository.Base;
using CRMService.Interfaces.Repository.Entity;
using CRMService.Interfaces.Repository.Extended;
using CRMService.Models.Entity;
using System.Linq.Expressions;

namespace CRMService.Repository.Entity
{
    public class IssuePriorityRepository(IGetItemByIdRepository<IssuePriority, int> _getById,
        IGetItemByCodeRepository<IssuePriority> _getByCode,
        ICreateItemRepository<IssuePriority> _create,
        IUpsertItemByCodeRepository<IssuePriority> _upsert) : IIssuePriorityRepository
    {
        public Task<IssuePriority?> GetItemById(int id, bool asNoTracking = false, CancellationToken ct = default, params Expression<Func<IssuePriority, object>>[] includes)
            => _getById.GetItemById(id, asNoTracking, ct, includes);

        public Task<List<IssuePriority>> GetItemsByPredicateAndSortById(Expression<Func<IssuePriority, bool>>? predicate = null, int skip = 0, int? take = null, bool asNoTracking = false, CancellationToken ct = default, params Expression<Func<IssuePriority, object>>[] includes)

        => _getById.GetItemsByPredicateAndSortById(predicate, skip, take, asNoTracking, ct, includes);

        public Task<IssuePriority?> GetItemByCode(string code, bool asNoTracking = false, CancellationToken ct = default, params Expression<Func<IssuePriority, object>>[] includes)
            => _getByCode.GetItemByCode(code, asNoTracking, ct, includes);

        public void Create(IssuePriority item) => _create.Create(item);

        public Task UpsertByCode(IssuePriority item, CancellationToken ct = default)
            => _upsert.UpsertByCode(item, ct);

        public Task UpsertByCode(string oldCode, IssuePriority item, CancellationToken ct = default)
            => _upsert.UpsertByCode(oldCode, item, ct);

        public Task UpsertByCodePairs(IEnumerable<(string OldCode, IssuePriority Item)> items, CancellationToken ct = default)
            => _upsert.UpsertByCodePairs(items, ct);

        public Task UpsertByCodes(IEnumerable<IssuePriority> items, CancellationToken ct = default)
            => _upsert.UpsertByCodes(items, ct);
    }
}
