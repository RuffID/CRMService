using Microsoft.EntityFrameworkCore;
using CRMService.Domain.Models.OkdeskEntity;
using EFCoreLibrary.Abstractions.Database.Repository.Base;

namespace CRMService.Application.Abstractions.Database.Repository.OkdeskEntity
{
    public interface IOkdeskIssuePriorityRepository :
        IGetItemByIdRepository<IssuePriority, int, DbContext>,
        IGetItemByPredicateRepository<IssuePriority, DbContext>
    {
    }
}
