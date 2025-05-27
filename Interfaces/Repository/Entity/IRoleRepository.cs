using CRMService.Interfaces.BaseRepository;
using CRMService.Models.Entity;

namespace CRMService.Interfaces.Repository.Entity
{
    public interface IRoleRepository : IGetRepository<Role>, IUpdateRepository<Role>, ICreateRepository<Role>
    {
        Task<Role?> GetRoleByName(string name, bool? trackable = null);
        public Task CreateOrUpdate(IEnumerable<Role> items);
    }
}