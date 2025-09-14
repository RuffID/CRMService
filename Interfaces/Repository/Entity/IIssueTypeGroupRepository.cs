using CRMService.Interfaces.Repository.Base;
using CRMService.Models.Entity;

namespace CRMService.Interfaces.Repository.Entity
{
    public interface IIssueTypeGroupRepository : IGetItemByIdRepository<IssueTypeGroup, int>, IGetItemByPredicateRepository<IssueTypeGroup>, IUpsertItemByIdRepository<IssueTypeGroup, int>, ICreateItemRepository<IssueTypeGroup>
    {
    }
}