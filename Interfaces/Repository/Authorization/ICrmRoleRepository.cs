using CRMService.Interfaces.BaseRepository;
using CRMService.Models.Authorization;

namespace CRMService.Interfaces.Repository.Authorization
{
    public interface ICrmRoleRepository : ICreateRepository<CrmRole>
    {
        Task<IEnumerable<CrmRole>?> GetItems(Range range);
        Task<ICollection<CrmRole>> GetItems(IEnumerable<CrmRole> items, bool trackable = true);
        Task<CrmRole?> GetItem(CrmRole item, bool? trackable = null);
    }
}
