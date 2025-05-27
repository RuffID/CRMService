using CRMService.Interfaces.BaseRepository;
using CRMService.Models.Entity;

namespace CRMService.Interfaces.Repository.Entity
{
    public interface ITimeEntryRepository : IGetRepository<TimeEntry>, IUpdateRepository<TimeEntry>, ICreateRepository<TimeEntry>, IDeleteRepository<TimeEntry>
    {
        Task<IEnumerable<TimeEntry>?> GetEntriesByIssue(int issueId, bool? trackable = null);
        public Task CreateOrUpdate(IEnumerable<TimeEntry> items);
    }
}