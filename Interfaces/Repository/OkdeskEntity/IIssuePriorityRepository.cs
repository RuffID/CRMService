using CRMService.Interfaces.Repository.Base;
using CRMService.Models.OkdeskEntity;

namespace CRMService.Interfaces.Repository.OkdeskEntity
{
    public interface IIssuePriorityRepository : IGetItemByIdRepository<IssuePriority, int>, IGetItemByPredicateRepository<IssuePriority>,  ICreateItemRepository<IssuePriority>
    {
    }
}