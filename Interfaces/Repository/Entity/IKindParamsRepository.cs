using CRMService.Interfaces.Repository.Base;
using CRMService.Models.Entity;

namespace CRMService.Interfaces.Repository.Entity
{
    public interface IKindParamsRepository : IGetItemByPredicateRepository<KindParam>, ICreateItemRepository<KindParam>, IDeleteItemRepository<KindParam>
    {
    }
}