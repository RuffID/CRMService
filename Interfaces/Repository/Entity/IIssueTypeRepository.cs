using CRMService.Interfaces.Repository.Base;
using CRMService.Models.Entity;

namespace CRMService.Interfaces.Repository.Entity
{
    public interface IIssueTypeRepository : IGetItemByIdRepository<IssueType, int>, IGetItemByPredicateRepository<IssueType>, IUpsertItemByCodeRepository<IssueType>, ICreateItemRepository<IssueType>
    {
    }
}