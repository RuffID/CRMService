using CRMService.Interfaces.Repository.Base;
using CRMService.Models.OkdeskEntity;

namespace CRMService.Interfaces.Repository.OkdeskEntity
{
    public interface IKindRepository : IGetItemByIdRepository<Kind, int>, IGetItemByPredicateRepository<Kind>, IUpsertItemByIdRepository<Kind, int>, ICreateItemRepository<Kind>
    {
    }
}