using CRMService.Abstractions.Database.Repository.Base;
using CRMService.Models.OkdeskEntity;

namespace CRMService.Abstractions.Database.Repository.OkdeskEntity
{
    public interface IIssuePriorityRepository : IGetItemByIdRepository<IssuePriority, int>, IGetItemByPredicateRepository<IssuePriority>,  ICreateItemRepository<IssuePriority>
    {
    }
}