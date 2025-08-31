using CRMService.Interfaces.Repository.Base;
using CRMService.Models.Entity;

namespace CRMService.Interfaces.Repository.Entity
{
    public interface ITimeEntryRepository : IGetItemByIdRepository<TimeEntry, int>, IUpsertItemByIdRepository<TimeEntry, int>, ICreateItemRepository<TimeEntry>, IDeleteItemRepository<TimeEntry>
    {
        Task<List<TimeEntry>> GetEntriesByIssue(int issueId, bool asNoTracking = false, CancellationToken ct = default);
    }
}