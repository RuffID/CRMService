using CRMService.Domain.Models.OkdeskEntity;
using EFCoreLibrary.Abstractions.Database.Repository.Base;
using Microsoft.EntityFrameworkCore;

namespace CRMService.Application.Abstractions.Database.Repository.OkdeskEntity
{
    public interface IOkdeskIssueRepository :
        IGetItemByIdRepository<Issue, int, DbContext>,
        IGetItemByPredicateRepository<Issue, DbContext>
    {
        Task<List<Issue>> GetUpdatedItemsAsync(DateTime dateFrom, DateTime dateTo, int startId, int limit, CancellationToken ct = default);
    }
}
