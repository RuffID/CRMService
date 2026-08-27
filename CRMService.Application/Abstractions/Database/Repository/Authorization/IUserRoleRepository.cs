using CRMService.Application.Abstractions.Database.Repository.Base;
using CRMService.Domain.Models.Authorization;

namespace CRMService.Application.Abstractions.Database.Repository.Authorization
{
    public interface IUserRoleRepository :
        IGetItemsRepository<UserRole>,
        ICreateItemRepository<UserRole>,
        IDeleteItemRepository<UserRole>
    {
    }
}
