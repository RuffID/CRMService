using Microsoft.EntityFrameworkCore;
using CRMService.Domain.Models.OkdeskEntity;
using EFCoreLibrary.Abstractions.Database.Repository.Base;

namespace CRMService.Application.Abstractions.Database.Repository.OkdeskEntity
{
    public interface IOkdeskIssueStatusRepository :
        IGetItemByIdRepository<IssueStatus, int, DbContext>,
        IGetItemByPredicateRepository<IssueStatus, DbContext>
    {
    }
}
