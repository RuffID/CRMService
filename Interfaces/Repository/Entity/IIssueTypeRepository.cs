using CRMService.Interfaces.Repository.Base;
using CRMService.Interfaces.Repository.Extended;
using CRMService.Models.Entity;

namespace CRMService.Interfaces.Repository.Entity
{
    public interface IIssueTypeRepository : IGetItemByIdRepository<IssueType, int>, IGetItemByCodeRepository<IssueType>, IUpsertItemByCodeRepository<IssueType>, ICreateItemRepository<IssueType>
    {
    }
}