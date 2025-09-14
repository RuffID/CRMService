using CRMService.Interfaces.Repository.Base;
using CRMService.Models.Entity;

namespace CRMService.Interfaces.Repository.Entity
{
    public interface IKindRepository : IGetItemByIdRepository<Kind, int>, IGetItemByPredicateRepository<Kind>, IUpsertItemByIdRepository<Kind, int>, ICreateItemRepository<Kind>
    {
    }
}