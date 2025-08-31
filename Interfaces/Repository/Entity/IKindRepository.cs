using CRMService.Interfaces.Repository.Base;
using CRMService.Interfaces.Repository.Extended;
using CRMService.Models.Entity;

namespace CRMService.Interfaces.Repository.Entity
{
    public interface IKindRepository : IGetItemByIdRepository<Kind, int>, IGetItemByCodeRepository<Kind>, IUpsertItemByCodeRepository<Kind>, ICreateItemRepository<Kind>
    {
    }
}