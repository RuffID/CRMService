using CRMService.Interfaces.Repository.Base;
using CRMService.Models.Entity;

namespace CRMService.Interfaces.Repository.Entity
{
    public interface IIssuePriorityRepository : IGetItemByIdRepository<IssuePriority, int>, IGetItemByPredicateRepository<IssuePriority>, IUpsertItemByIdRepository<IssuePriority, int>, ICreateItemRepository<IssuePriority>
    {
    }
}