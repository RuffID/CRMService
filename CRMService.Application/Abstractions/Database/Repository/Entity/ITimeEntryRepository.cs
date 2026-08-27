using CRMService.Application.Abstractions.Database.Repository.Base;
using CRMService.Domain.Models.OkdeskEntity;

namespace CRMService.Application.Abstractions.Database.Repository.Entity
{
    public interface ITimeEntryRepository :
        IGetItemByIdRepository<TimeEntry, int>,
        ICreateItemRepository<TimeEntry>,
        IDeleteItemRepository<TimeEntry>
    {
        Task<List<TimeEntry>> GetByIssueIdAsync(int issueId, CancellationToken ct = default);
        Task<List<TimeEntry>> GetByIssueIdReadOnlyAsync(int issueId, CancellationToken ct = default);
        Task<List<TimeEntry>> GetMissingFromCloudAsync(int issueId, IReadOnlyCollection<int> cloudIds, CancellationToken ct = default);
    }
}
