using Microsoft.EntityFrameworkCore;
using EFCoreLibrary.Abstractions.Database.Repository.Base;
using CRMService.Domain.Models.OkdeskEntity;

namespace CRMService.Application.Abstractions.Database.Repository.Entity
{
    public interface IIssueStatusRepository :
        IGetItemByIdRepository<IssueStatus, int, DbContext>,
        IGetItemByPredicateRepository<IssueStatus, DbContext>,
        ICreateItemRepository<IssueStatus, DbContext>
    {
    }
}