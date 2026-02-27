using EFCoreLibrary.Abstractions.Database.Repository.Base;
using CRMService.Models.OkdeskEntity;

namespace CRMService.Abstractions.Database.Repository.OkdeskEntity
{
    public interface IIssueRepository : IGetItemByIdRepository<Issue, int>, IGetItemByPredicateRepository<Issue>, ICreateItemRepository<Issue>
    {
    }
}