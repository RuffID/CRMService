using EFCoreLibrary.Abstractions.Database.Repository.Base;
using CRMService.Domain.Models.OkdeskEntity;

namespace CRMService.Application.Abstractions.Database.Repository.OkdeskEntity
{
    public interface IIssueTypeRepository : IGetItemByIdRepository<IssueType, int>, IGetItemByPredicateRepository<IssueType>, ICreateItemRepository<IssueType>
    {
    }
}


