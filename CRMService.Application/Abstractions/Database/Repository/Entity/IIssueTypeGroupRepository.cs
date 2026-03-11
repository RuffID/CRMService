using Microsoft.EntityFrameworkCore;
using EFCoreLibrary.Abstractions.Database.Repository.Base;
using CRMService.Domain.Models.OkdeskEntity;

namespace CRMService.Application.Abstractions.Database.Repository.Entity
{
    public interface IIssueTypeGroupRepository :
        IGetItemByIdRepository<IssueTypeGroup, int, DbContext>,
        IGetItemByPredicateRepository<IssueTypeGroup, DbContext>,
        ICreateItemRepository<IssueTypeGroup, DbContext>
    {
    }
}