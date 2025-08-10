using CRMService.Interfaces.BaseRepository;
using CRMService.Models.Authorization;

namespace CRMService.Interfaces.Repository.Authorization
{
    public interface IRoleRepository : ICreateRepository<Role>
    {
        Task<IEnumerable<Role>?> GetItems(Range range);
        Task<ICollection<Role>> GetItems(IEnumerable<Role> items, bool trackable = true);
        Task<Role?> GetItem(Role item, bool? trackable = null);
    }
}
