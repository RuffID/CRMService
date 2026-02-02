using CRMService.Interfaces.Repository.Base;
using CRMService.Models.OkdeskEntity;

namespace CRMService.Interfaces.Repository.OkdeskEntity
{
    public interface IIssueRepository : IGetItemByIdRepository<Issue, int>, IGetItemByPredicateRepository<Issue>, ICreateItemRepository<Issue>
    {
    }
}