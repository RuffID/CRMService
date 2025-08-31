using CRMService.Interfaces.Repository.Base;
using CRMService.Interfaces.Repository.Extended;
using CRMService.Models.Authorization;

namespace CRMService.Interfaces.Repository.Authorization
{
    public interface IUserRepository : IGetItemByIdRepository<User, Guid>, IGetItemByPredicateRepository<User>, ICreateItemRepository<User>, IUpsertItemByIdRepository<User, Guid>
    {
    }
}
