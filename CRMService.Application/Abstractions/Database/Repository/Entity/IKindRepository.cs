using Microsoft.EntityFrameworkCore;
using EFCoreLibrary.Abstractions.Database.Repository.Base;
using CRMService.Domain.Models.OkdeskEntity;

namespace CRMService.Application.Abstractions.Database.Repository.Entity
{
    public interface IKindRepository :
        IGetItemByIdRepository<Kind, int, DbContext>,
        IGetItemByPredicateRepository<Kind, DbContext>,
        ICreateItemRepository<Kind, DbContext>
    {
    }
}