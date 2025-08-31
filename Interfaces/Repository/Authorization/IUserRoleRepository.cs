using CRMService.Interfaces.Repository.Base;
using CRMService.Interfaces.Repository.Extended;
using CRMService.Models.Authorization;

namespace CRMService.Interfaces.Repository.Authorization
{
    public interface IUserRoleRepository : IGetItemByPredicateRepository<UserRole>, ICreateItemRepository<UserRole>, IDeleteItemRepository<UserRole>
    {
    }
}
