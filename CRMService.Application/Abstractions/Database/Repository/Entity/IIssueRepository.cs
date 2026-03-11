using Microsoft.EntityFrameworkCore;
using EFCoreLibrary.Abstractions.Database.Repository.Base;
using CRMService.Domain.Models.OkdeskEntity;

namespace CRMService.Application.Abstractions.Database.Repository.Entity
{
    public interface IIssueRepository :
        IGetItemByIdRepository<Issue, int, DbContext>,
        IGetItemByPredicateRepository<Issue, DbContext>,
        ICreateItemRepository<Issue, DbContext>
    {
    }
}