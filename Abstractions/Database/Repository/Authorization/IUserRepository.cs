using CRMService.Abstractions.Database.Repository.Base;
using CRMService.Models.Authorization;

namespace CRMService.Abstractions.Database.Repository.Authorization
{
    public interface IUserRepository : IGetItemByIdRepository<User, Guid>, IGetItemByPredicateRepository<User>, ICreateItemRepository<User>
    {
    }
}
