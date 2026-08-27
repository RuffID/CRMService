using CRMService.Application.Abstractions.Database.Repository.Authorization;
using CRMService.Application.Abstractions.Database.Repository.Entity;

namespace CRMService.Application.Abstractions.Database.Repository
{
    public interface IAuthorizationUnitOfWork : IUnitOfWorkScope
    {
        IUserRepository User { get; }
        ICrmRoleRepository CrmRole { get; }
        ISessionRepository Session { get; }
        IEmployeeRepository Employee { get; }
    }
}
