using Microsoft.EntityFrameworkCore;
using EFCoreLibrary.Abstractions.Database.Repository.Base;
using CRMService.Domain.Models.Authorization;

namespace CRMService.Application.Abstractions.Database.Repository.Authorization
{
    public interface ISessionRepository :
        IGetItemByIdRepository<Session, Guid, DbContext>,
        IGetItemByPredicateRepository<Session, DbContext>,
        ICreateItemRepository<Session, DbContext>,
        IDeleteItemRepository<Session, DbContext>
    {
    }
}