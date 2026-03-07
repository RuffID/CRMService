using Microsoft.EntityFrameworkCore;
using EFCoreLibrary.Abstractions.Database.Repository.Base;
using CRMService.Domain.Models.Authorization;

namespace CRMService.Application.Abstractions.Database.Repository.Authorization
{
    public interface IBlockReasonRepository :
        IGetItemByIdRepository<BlockReason, Guid, DbContext>,
        IGetItemByPredicateRepository<BlockReason, DbContext>,
        ICreateItemRepository<BlockReason, DbContext>
    {
    }
}