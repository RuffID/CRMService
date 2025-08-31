using CRMService.Interfaces.Repository.Base;
using CRMService.Interfaces.Repository.Entity;
using CRMService.Models.Entity;
using System.Linq.Expressions;

namespace CRMService.Repository.Entity
{
    public class TimeEntryRepository(IGetItemByIdRepository<TimeEntry, int> _getById,
        ICreateItemRepository<TimeEntry> _create,
        IUpsertItemByIdRepository<TimeEntry, int> _upsert,
        IDeleteItemRepository<TimeEntry> _delete) : ITimeEntryRepository
    {
        public Task<TimeEntry?> GetItemById(int id, bool asNoTracking = false, CancellationToken ct = default, params Expression<Func<TimeEntry, object>>[] includes)
            => _getById.GetItemById(id, asNoTracking, ct, includes);

        public Task<List<TimeEntry>> GetItemsByPredicateAndSortById(Expression<Func<TimeEntry, bool>>? predicate = null, int skip = 0, int? take = null, bool asNoTracking = false, CancellationToken ct = default, params Expression<Func<TimeEntry, object>>[] includes)
            => _getById.GetItemsByPredicateAndSortById(predicate, skip, take, asNoTracking, ct, includes);

        public Task<List<TimeEntry>> GetEntriesByIssue(int issueId, bool asNoTracking = false, CancellationToken ct = default)
            => _getById.GetItemsByPredicateAndSortById(te => te.IssueId == issueId, asNoTracking: asNoTracking, ct: ct);

        public void Create(TimeEntry item) => _create.Create(item);

        public Task Upsert(TimeEntry item, CancellationToken ct = default)
            => _upsert.Upsert(item, ct);

        public Task Upsert(IEnumerable<TimeEntry> items, CancellationToken ct = default)
            => _upsert.Upsert(items, ct);

        public void Delete(TimeEntry item) => _delete.Delete(item);
    }
}
