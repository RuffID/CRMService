using CRMService.DataBase;
using CRMService.Interfaces.Repository.Base;
using CRMService.Interfaces.Repository.Entity;
using CRMService.Interfaces.Repository.Extended;
using CRMService.Models.Entity;
using System.Linq.Expressions;

namespace CRMService.Repository.Entity
{
    public class IssueStatusRepository(IGetItemByIdRepository<IssueStatus, int> _getById,
        IGetItemByCodeRepository<IssueStatus> _getByCode,
        ICreateItemRepository<IssueStatus> _create,
        IUpsertItemByCodeRepository<IssueStatus> _upsert) : IIssueStatusRepository
    {
        public Task<IssueStatus?> GetItemById(int id, bool asNoTracking = false, CancellationToken ct = default, params Expression<Func<IssueStatus, object>>[] includes)
            => _getById.GetItemById(id, asNoTracking, ct, includes);

        public Task<List<IssueStatus>> GetItemsByPredicateAndSortById(Expression<Func<IssueStatus, bool>>? predicate = null, int skip = 0, int? take = null, bool asNoTracking = false, CancellationToken ct = default, params Expression<Func<IssueStatus, object>>[] includes)

        => _getById.GetItemsByPredicateAndSortById(predicate, skip, take, asNoTracking, ct, includes);

        public Task<IssueStatus?> GetItemByCode(string code, bool asNoTracking = false, CancellationToken ct = default, params Expression<Func<IssueStatus, object>>[] includes)
            => _getByCode.GetItemByCode(code, asNoTracking, ct, includes);

        public void Create(IssueStatus item) => _create.Create(item);

        public Task UpsertByCode(IssueStatus item, CancellationToken ct = default)
            => _upsert.UpsertByCode(item, ct);

        public Task UpsertByCode(string oldCode, IssueStatus item, CancellationToken ct = default)
            => _upsert.UpsertByCode(oldCode, item, ct);

        public Task UpsertByCodePairs(IEnumerable<(string OldCode, IssueStatus Item)> items, CancellationToken ct = default)
            => _upsert.UpsertByCodePairs(items, ct);

        public Task UpsertByCodes(IEnumerable<IssueStatus> items, CancellationToken ct = default)
            => _upsert.UpsertByCodes(items, ct);
    }
}
