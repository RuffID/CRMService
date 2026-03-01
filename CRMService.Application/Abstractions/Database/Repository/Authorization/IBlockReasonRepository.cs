using EFCoreLibrary.Abstractions.Database.Repository.Base;
using CRMService.Domain.Models.Authorization;

namespace CRMService.Application.Abstractions.Database.Repository.Authorization
{
    public interface IBlockReasonRepository : IGetItemByIdRepository<BlockReason, Guid>, IGetItemByPredicateRepository<BlockReason>, ICreateItemRepository<BlockReason>
    {
    }
}



