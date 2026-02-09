using CRMService.Abstractions.Database.Repository.Base;
using CRMService.Models.OkdeskEntity;

namespace CRMService.Abstractions.Database.Repository.OkdeskEntity
{
    public interface ITimeEntryRepository : IGetItemByIdRepository<TimeEntry, int>, IGetItemByPredicateRepository<TimeEntry>, ICreateItemRepository<TimeEntry>, IDeleteItemRepository<TimeEntry>
    {
    }
}