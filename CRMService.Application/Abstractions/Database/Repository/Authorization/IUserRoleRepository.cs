using EFCoreLibrary.Abstractions.Database.Repository.Base;
using CRMService.Domain.Models.Authorization;

namespace CRMService.Application.Abstractions.Database.Repository.Authorization
{
    public interface IUserRoleRepository : IGetItemByPredicateRepository<UserRole>, ICreateItemRepository<UserRole>, IDeleteItemRepository<UserRole>
    {
    }
}



