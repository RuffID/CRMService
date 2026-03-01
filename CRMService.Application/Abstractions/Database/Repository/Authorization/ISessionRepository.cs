using EFCoreLibrary.Abstractions.Database.Repository.Base;
using CRMService.Domain.Models.Authorization;

namespace CRMService.Application.Abstractions.Database.Repository.Authorization
{
    public interface ISessionRepository : IGetItemByIdRepository<Session, Guid>, IGetItemByPredicateRepository<Session>, ICreateItemRepository<Session>, IDeleteItemRepository<Session>
    {
    }
}



