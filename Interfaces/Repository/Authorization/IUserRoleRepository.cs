using CRMService.Interfaces.BaseRepository;
using CRMService.Models.Authorization;

namespace CRMService.Interfaces.Repository.Authorization
{
    public interface IUserRoleRepository : ICreateRepository<UserRole>, IDeleteRepository<UserRole>, IUpdateRepository<UserRole>
    {
        Task<IEnumerable<UserRole>?> GetAllItem(Range range);
        Task<UserRole?> GetItem(UserRole item, bool? trackable = null);
        Task<IEnumerable<Role>?> GetRolesByUserId(Guid userId);
        Task<UserRole?> GetConnectionByUserAndRoleId(UserRole item, bool? trackable = null);
    }
}
