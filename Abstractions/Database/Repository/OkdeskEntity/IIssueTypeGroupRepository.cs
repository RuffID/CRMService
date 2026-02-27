using EFCoreLibrary.Abstractions.Database.Repository.Base;
using CRMService.Models.OkdeskEntity;

namespace CRMService.Abstractions.Database.Repository.OkdeskEntity
{
    public interface IIssueTypeGroupRepository : IGetItemByIdRepository<IssueTypeGroup, int>, IGetItemByPredicateRepository<IssueTypeGroup>,  ICreateItemRepository<IssueTypeGroup>
    {
    }
}