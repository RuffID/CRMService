using EFCoreLibrary.Abstractions.Database.Repository.Base;
using CRMService.Domain.Models.OkdeskEntity;

namespace CRMService.Application.Abstractions.Database.Repository.OkdeskEntity
{
    public interface IIssueTypeGroupRepository : IGetItemByIdRepository<IssueTypeGroup, int>, IGetItemByPredicateRepository<IssueTypeGroup>,  ICreateItemRepository<IssueTypeGroup>
    {
    }
}


