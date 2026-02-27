using EFCoreLibrary.Abstractions.Database.Repository.Base;
using CRMService.Models.Authorization;

namespace CRMService.Abstractions.Database.Repository.Authorization
{
    public interface ISessionRepository : IGetItemByIdRepository<Session, Guid>, IGetItemByPredicateRepository<Session>, ICreateItemRepository<Session>, IDeleteItemRepository<Session>
    {
    }
}
