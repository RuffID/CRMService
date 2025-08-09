using CRMService.Interfaces.BaseRepository;
using CRMService.Models.Authorization;

namespace CRMService.Interfaces.Repository.Authorization
{
    public interface IRoleRepository : ICreateRepository<Role>
    {
        Task<IEnumerable<Role>?> GetAllItem(Range range);
        Task<Role?> GetItem(Role item, bool? trackable = null);
    }
}
