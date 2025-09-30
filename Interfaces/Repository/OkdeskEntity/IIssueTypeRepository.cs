using CRMService.Interfaces.Repository.Base;
using CRMService.Models.OkdeskEntity;

namespace CRMService.Interfaces.Repository.OkdeskEntity
{
    public interface IIssueTypeRepository : IGetItemByIdRepository<IssueType, int>, IGetItemByPredicateRepository<IssueType>, IUpsertItemByIdRepository<IssueType, int>, ICreateItemRepository<IssueType>
    {
    }
}