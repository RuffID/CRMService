using Microsoft.EntityFrameworkCore;
using EFCoreLibrary.Abstractions.Database.Repository.Base;
using CRMService.Domain.Models.Authorization;

namespace CRMService.Application.Abstractions.Database.Repository.Authorization
{
    public interface IUserRepository :
        IGetItemByIdRepository<User, Guid, DbContext>,
        IGetItemByPredicateRepository<User, DbContext>,
        ICreateItemRepository<User, DbContext>
    {
    }
}