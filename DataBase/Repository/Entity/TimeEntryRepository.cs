using CRMService.Interfaces.Repository.Base;
using CRMService.Interfaces.Repository.Entity;
using CRMService.Models.Entity;
using System.Linq.Expressions;

namespace CRMService.DataBase.Repository.Entity
{
    public class TimeEntryRepository(IGetItemByIdRepository<TimeEntry, int> getItemById,
        ICreateItemRepository<TimeEntry> create,
        IUpsertItemByIdRepository<TimeEntry, int> upsert,
        IDeleteItemRepository<TimeEntry> delete) : ITimeEntryRepository
    {
        public Task<TimeEntry?> GetItemById(int id, bool asNoTracking = false, CancellationToken ct = default, params Expression<Func<TimeEntry, object>>[] includes)
            => getItemById.GetItemById(id, asNoTracking, ct, includes);

        public Task<List<TimeEntry>> GetItemsByPredicateAndSortById(Expression<Func<TimeEntry, bool>>? predicate = null, int skip = 0, int? take = null, bool asNoTracking = false, CancellationToken ct = default, params Expression<Func<TimeEntry, object>>[] includes)
            => getItemById.GetItemsByPredicateAndSortById(predicate, skip, take, asNoTracking, ct, includes);

        public Task<List<TimeEntry>> GetEntriesByIssue(int issueId, bool asNoTracking = false, CancellationToken ct = default)
            => getItemById.GetItemsByPredicateAndSortById(te => te.IssueId == issueId, asNoTracking: asNoTracking, ct: ct);

        public void Create(TimeEntry item) => create.Create(item);

        public Task Upsert(TimeEntry item, CancellationToken ct = default)
            => upsert.Upsert(item, ct);

        public Task Upsert(IEnumerable<TimeEntry> items, CancellationToken ct = default)
            => upsert.Upsert(items, ct);

        public void Delete(TimeEntry item) => delete.Delete(item);
    }
}
