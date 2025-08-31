using CRMService.Interfaces.Repository.Base;
using CRMService.Interfaces.Repository.Entity;
using CRMService.Interfaces.Repository.Extended;
using CRMService.Models.Entity;
using System.Linq.Expressions;

namespace CRMService.Repository.Entity
{
    public class IssueTypeRepository(IGetItemByIdRepository<IssueType, int> _getById,
        IGetItemByCodeRepository<IssueType> _getByCode,
        ICreateItemRepository<IssueType> _create,
        IUpsertItemByCodeRepository<IssueType> _upsert) : IIssueTypeRepository
    {
        public Task<IssueType?> GetItemById(int id, bool asNoTracking = false, CancellationToken ct = default, params Expression<Func<IssueType, object>>[] includes)
            => _getById.GetItemById(id, asNoTracking, ct, includes);

        public Task<List<IssueType>> GetItemsByPredicateAndSortById(Expression<Func<IssueType, bool>>? predicate = null, int skip = 0, int? take = null, bool asNoTracking = false, CancellationToken ct = default, params Expression<Func<IssueType, object>>[] includes)

        => _getById.GetItemsByPredicateAndSortById(predicate, skip, take, asNoTracking, ct, includes);

        public Task<IssueType?> GetItemByCode(string code, bool asNoTracking = false, CancellationToken ct = default, params Expression<Func<IssueType, object>>[] includes)
            => _getByCode.GetItemByCode(code, asNoTracking, ct, includes);

        public void Create(IssueType item) => _create.Create(item);

        public Task UpsertByCode(IssueType item, CancellationToken ct = default)
            => _upsert.UpsertByCode(item, ct);

        public Task UpsertByCode(string oldCode, IssueType item, CancellationToken ct = default)
            => _upsert.UpsertByCode(oldCode, item, ct);

        public Task UpsertByCodePairs(IEnumerable<(string OldCode, IssueType Item)> items, CancellationToken ct = default)
            => _upsert.UpsertByCodePairs(items, ct);

        public Task UpsertByCodes(IEnumerable<IssueType> items, CancellationToken ct = default)
            => _upsert.UpsertByCodes(items, ct);
    }
}
