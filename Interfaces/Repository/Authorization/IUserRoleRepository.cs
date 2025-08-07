using CRMService.Interfaces.BaseRepository;
using CRMService.Models.Authorization;

namespace CRMService.Interfaces.Repository.Auth
{
    public interface IUserRoleRepository : IGetRepository<UserRole>, ICreateRepository<UserRole>, IDeleteRepository<UserRole>, IUpdateRepository<UserRole>
    {
        Task<IEnumerable<Role>?> GetRolesByUserId(Guid userId);
        Task<UserRole?> GetConnectionByUserAndRoleId(UserRole item);

    }
}
