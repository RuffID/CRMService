using CRMService.Abstractions.Database.Repository.Base;
using CRMService.Models.Authorization;

namespace CRMService.Abstractions.Database.Repository.Authorization
{
    public interface IBlockReasonRepository : IGetItemByIdRepository<BlockReason, Guid>, IGetItemByPredicateRepository<BlockReason>, ICreateItemRepository<BlockReason>
    {
    }
}
