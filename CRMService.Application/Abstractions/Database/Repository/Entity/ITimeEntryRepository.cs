using Microsoft.EntityFrameworkCore;
using EFCoreLibrary.Abstractions.Database.Repository.Base;
using CRMService.Domain.Models.OkdeskEntity;

namespace CRMService.Application.Abstractions.Database.Repository.Entity
{
    public interface ITimeEntryRepository :
        IGetItemByIdRepository<TimeEntry, int, DbContext>,
        IGetItemByPredicateRepository<TimeEntry, DbContext>,
        ICreateItemRepository<TimeEntry, DbContext>,
        IDeleteItemRepository<TimeEntry, DbContext>
    {
    }
}