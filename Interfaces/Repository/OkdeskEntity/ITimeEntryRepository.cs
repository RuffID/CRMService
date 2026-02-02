using CRMService.Interfaces.Repository.Base;
using CRMService.Models.OkdeskEntity;

namespace CRMService.Interfaces.Repository.OkdeskEntity
{
    public interface ITimeEntryRepository : IGetItemByIdRepository<TimeEntry, int>, IGetItemByPredicateRepository<TimeEntry>, ICreateItemRepository<TimeEntry>, IDeleteItemRepository<TimeEntry>
    {
    }
}