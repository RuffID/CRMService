using CRMService.Interfaces.Repository.Base;
using CRMService.Models.Entity;

namespace CRMService.Interfaces.Repository.Entity
{
    public interface IKindRepository : IGetItemByIdRepository<Kind, int>, IGetItemByPredicateRepository<Kind>, IUpsertItemByCodeRepository<Kind>, ICreateItemRepository<Kind>
    {
    }
}