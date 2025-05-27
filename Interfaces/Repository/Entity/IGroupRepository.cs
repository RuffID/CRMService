using CRMService.Interfaces.BaseRepository;
using CRMService.Models.Entity;

namespace CRMService.Interfaces.Repository.Entity
{
    public interface IGroupRepository : IGetRepository<Group>, IUpdateRepository<Group>, ICreateRepository<Group>
    {
        public Task CreateOrUpdate(IEnumerable<Group> items);
    }
}