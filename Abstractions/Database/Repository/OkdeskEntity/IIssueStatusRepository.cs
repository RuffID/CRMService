using CRMService.Abstractions.Database.Repository.Base;
using CRMService.Models.OkdeskEntity;

namespace CRMService.Abstractions.Database.Repository.OkdeskEntity
{
    public interface IIssueStatusRepository : IGetItemByIdRepository<IssueStatus, int>, IGetItemByPredicateRepository<IssueStatus>,  ICreateItemRepository<IssueStatus>
    {
    }
}