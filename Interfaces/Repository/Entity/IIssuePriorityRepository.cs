using CRMService.Interfaces.BaseRepository;
using CRMService.Models.Entity;

namespace CRMService.Interfaces.Repository.Entity
{
    public interface IIssuePriorityRepository : IGetRepository<IssuePriority>, IUpdateRepository<IssuePriority>, ICreateRepository<IssuePriority>
    {
        public Task CreateOrUpdate(IEnumerable<IssuePriority> items);
        public Task<int> GetCountOfItems();
    }
}