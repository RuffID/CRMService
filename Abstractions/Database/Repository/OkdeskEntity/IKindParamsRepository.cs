using CRMService.Abstractions.Database.Repository.Base;
using CRMService.Models.OkdeskEntity;

namespace CRMService.Abstractions.Database.Repository.OkdeskEntity
{
    public interface IKindParamsRepository : IGetItemByPredicateRepository<KindParam>, ICreateItemRepository<KindParam>, IDeleteItemRepository<KindParam>
    {
    }
}