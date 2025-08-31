using CRMService.Interfaces.Repository.Base;
using CRMService.Interfaces.Repository.Extended;
using CRMService.Models.Entity;

namespace CRMService.Interfaces.Repository.Entity
{
    public interface IIssueStatusRepository : IGetItemByIdRepository<IssueStatus, int>, IGetItemByCodeRepository<IssueStatus>, IUpsertItemByCodeRepository<IssueStatus>, ICreateItemRepository<IssueStatus>
    {
    }
}