using CRMService.Interfaces.Repository.Base;
using CRMService.Models.Authorization;

namespace CRMService.Interfaces.Repository.Authorization
{
    public interface IBlockReasonRepository : IGetItemByIdRepository<BlockReason, Guid>, IGetItemByPredicateRepository<BlockReason>, ICreateItemRepository<BlockReason>, IUpsertItemByIdRepository<BlockReason, Guid>
    {
    }
}
