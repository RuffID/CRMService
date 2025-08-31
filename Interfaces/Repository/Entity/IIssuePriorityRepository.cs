using CRMService.Interfaces.Repository.Base;
using CRMService.Interfaces.Repository.Extended;
using CRMService.Models.Entity;

namespace CRMService.Interfaces.Repository.Entity
{
    public interface IIssuePriorityRepository : IGetItemByIdRepository<IssuePriority, int>, IGetItemByCodeRepository<IssuePriority>, IUpsertItemByCodeRepository<IssuePriority>, ICreateItemRepository<IssuePriority>
    {
    }
}