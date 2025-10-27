using CRMService.Interfaces.Repository.Base;
using CRMService.Models.OkdeskEntity;

namespace CRMService.Interfaces.Repository.OkdeskEntity
{
    public interface ITimeEntryRepository : IGetItemByIdRepository<TimeEntry, int>, IUpsertItemByIdRepository<TimeEntry, int>, ICreateItemRepository<TimeEntry>, IDeleteItemRepository<TimeEntry>
    {
        Task<List<TimeEntry>> GetEntriesByIssue(int issueId, bool asNoTracking = false, CancellationToken ct = default);
        Task<List<int>> GetItemIdsByCloudIdsFromIssueId(int issueId, List<int> cloudIds, CancellationToken ct);
        Task DeleteAllByIssueId(int issueId, CancellationToken ct);
    }
}