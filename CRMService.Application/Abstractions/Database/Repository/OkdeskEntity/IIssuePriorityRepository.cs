using Microsoft.EntityFrameworkCore;
using EFCoreLibrary.Abstractions.Database.Repository.Base;
using CRMService.Domain.Models.OkdeskEntity;

namespace CRMService.Application.Abstractions.Database.Repository.OkdeskEntity
{
    public interface IIssuePriorityRepository :
        IGetItemByIdRepository<IssuePriority, int, DbContext>,
        IGetItemByPredicateRepository<IssuePriority, DbContext>,
        ICreateItemRepository<IssuePriority, DbContext>
    {
    }
}