using CRMService.Application.Abstractions.Database.Repository;
using CRMService.Application.Abstractions.Database.Repository.Authorization;
using CRMService.Application.Abstractions.Database.Repository.Entity;

namespace CRMService.Infrastructure.DataBase.Repository
{
    public class AuthorizationUnitOfWork(
        IUnitOfWorkScope scope,
        IUserRepository user,
        ICrmRoleRepository crmRole,
        ISessionRepository session,
        IEmployeeRepository employee) : MainScenarioUnitOfWork(scope), IAuthorizationUnitOfWork
    {
        public IUserRepository User { get; } = user;
        public ICrmRoleRepository CrmRole { get; } = crmRole;
        public ISessionRepository Session { get; } = session;
        public IEmployeeRepository Employee { get; } = employee;
    }
}
