using CRMService.Interfaces.Repository.Base;
using CRMService.Models.OkdeskEntity;

namespace CRMService.Interfaces.Repository.OkdeskEntity
{
    public interface IIssueStatusRepository : IGetItemByIdRepository<IssueStatus, int>, IGetItemByPredicateRepository<IssueStatus>, IUpsertItemByIdRepository<IssueStatus, int>, ICreateItemRepository<IssueStatus>
    {
    }
}