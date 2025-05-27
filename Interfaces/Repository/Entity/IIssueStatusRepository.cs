using CRMService.Interfaces.BaseRepository;
using CRMService.Models.Entity;

namespace CRMService.Interfaces.Repository.Entity
{
    public interface IIssueStatusRepository : IGetRepository<IssueStatus>, IUpdateRepository<IssueStatus>, ICreateRepository<IssueStatus>
    {
        public Task CreateOrUpdate(IEnumerable<IssueStatus> items);
        public Task<int> GetCountOfItems();
    }
}