using Microsoft.EntityFrameworkCore;
using EFCoreLibrary.Abstractions.Database.Repository.Base;
using CRMService.Domain.Models.OkdeskEntity;

namespace CRMService.Application.Abstractions.Database.Repository.Entity
{
    public interface IKindParamsRepository :
        IGetItemByPredicateRepository<KindParam, DbContext>,
        ICreateItemRepository<KindParam, DbContext>,
        IDeleteItemRepository<KindParam, DbContext>
    {
    }
}