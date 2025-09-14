using CRMService.Interfaces.Repository.Base;
using CRMService.Models.Entity;

namespace CRMService.Interfaces.Repository.Entity
{
    public interface IIssueStatusRepository : IGetItemByIdRepository<IssueStatus, int>, IGetItemByPredicateRepository<IssueStatus>, IUpsertItemByIdRepository<IssueStatus, int>, ICreateItemRepository<IssueStatus>
    {
    }
}