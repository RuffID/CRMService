using EFCoreLibrary.Abstractions.Database.Repository.Base;
using CRMService.Domain.Models.OkdeskEntity;

namespace CRMService.Application.Abstractions.Database.Repository.OkdeskEntity
{
    public interface IKindParamsRepository : IGetItemByPredicateRepository<KindParam>, ICreateItemRepository<KindParam>, IDeleteItemRepository<KindParam>
    {
    }
}


