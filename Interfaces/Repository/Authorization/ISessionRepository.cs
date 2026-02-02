using CRMService.Interfaces.Repository.Base;
using CRMService.Models.Authorization;

namespace CRMService.Interfaces.Repository.Authorization
{
    public interface ISessionRepository : IGetItemByIdRepository<Session, Guid>, IGetItemByPredicateRepository<Session>, ICreateItemRepository<Session>, IDeleteItemRepository<Session>
    {
    }
}
