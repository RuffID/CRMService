using CRMService.Interfaces.Repository.Base;
using CRMService.Models.OkdeskEntity;

namespace CRMService.Interfaces.Repository.OkdeskEntity
{
    public interface IIssueTypeGroupRepository : IGetItemByIdRepository<IssueTypeGroup, int>, IGetItemByPredicateRepository<IssueTypeGroup>, IUpsertItemByIdRepository<IssueTypeGroup, int>, ICreateItemRepository<IssueTypeGroup>
    {
    }
}