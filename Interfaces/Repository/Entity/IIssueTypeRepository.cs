using CRMService.Interfaces.BaseRepository;
using CRMService.Models.Entity;

namespace CRMService.Interfaces.Repository.Entity
{
    public interface IIssueTypeRepository : IGetRepository<IssueType>, IUpdateRepository<IssueType>, ICreateRepository<IssueType>
    {
        public Task CreateOrUpdate(IEnumerable<IssueType> items);
        public Task<int> GetCountOfItems();
    }
}