using CRMService.Interfaces.BaseRepository;
using CRMService.Models.Entity;

namespace CRMService.Interfaces.Repository.Entity
{
    public interface IRoleRepository : IGetRepository<OkdeskRole>, IUpdateRepository<OkdeskRole>, ICreateRepository<OkdeskRole>
    {
        Task<OkdeskRole?> GetRoleByName(string name, bool? trackable = null);
        public Task CreateOrUpdate(IEnumerable<OkdeskRole> items);
    }
}