using CRMService.Interfaces.BaseRepository;
using CRMService.Models.Authorization;

namespace CRMService.Interfaces.Repository.Authorization
{
    public interface IUserRepository : ICreateRepository<User>, IUpdateRepository<User>
    {
        Task<IEnumerable<User>?> GetAllItem(Range range);
        Task<User?> GetItem(User user, bool? trackable = null);
        Task<User?> GetUserWithRoles(User user, bool? trackable = null);
    }
}
