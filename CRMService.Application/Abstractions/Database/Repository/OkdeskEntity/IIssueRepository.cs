using EFCoreLibrary.Abstractions.Database.Repository.Base;
using CRMService.Domain.Models.OkdeskEntity;

namespace CRMService.Application.Abstractions.Database.Repository.OkdeskEntity
{
    public interface IIssueRepository : IGetItemByIdRepository<Issue, int>, IGetItemByPredicateRepository<Issue>, ICreateItemRepository<Issue>
    {
    }
}


