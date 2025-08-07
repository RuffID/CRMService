using CRMService.Interfaces.Repository.Auth;
using CRMService.Interfaces.Repository.Authorization;

namespace CRMService.Interfaces.Repository
{
    public interface IUnitOfWorkAuthorization
    {
        IBlockReasonRepository BlockReason { get; }

        IRoleRepository Role { get; }

        ISessionRepository Session { get; }

        IUserRepository User { get; }

        IUserRoleRepository UserRole { get; }

        Task SaveAsync();
    }
}
