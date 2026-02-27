using EFCoreLibrary.Abstractions.Database.Repository.Base;
using CRMService.Models.Authorization;

namespace CRMService.Abstractions.Database.Repository.Authorization
{
    public interface IUserRoleRepository : IGetItemByPredicateRepository<UserRole>, ICreateItemRepository<UserRole>, IDeleteItemRepository<UserRole>
    {
    }
}
