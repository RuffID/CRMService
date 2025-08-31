using CRMService.Interfaces.Repository.Base;
using CRMService.Interfaces.Repository.Extended;
using CRMService.Models.Authorization;

namespace CRMService.Interfaces.Repository.Authorization
{
    public interface ISessionRepository : IGetItemByIdRepository<Session, Guid>, IGetItemByPredicateRepository<Session>, ICreateItemRepository<Session>, IUpsertItemByIdRepository<Session, Guid>, IDeleteItemRepository<Session>
    {
    }
}
