using EFCoreLibrary.Abstractions.Database.Repository.Base;
using CRMService.Domain.Models.OkdeskEntity;

namespace CRMService.Application.Abstractions.Database.Repository.OkdeskEntity
{
    public interface ITimeEntryRepository : IGetItemByIdRepository<TimeEntry, int>, IGetItemByPredicateRepository<TimeEntry>, ICreateItemRepository<TimeEntry>, IDeleteItemRepository<TimeEntry>
    {
    }
}


