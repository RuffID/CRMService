using CRMService.Domain.Models.OkdeskEntity;
using EFCoreLibrary.Abstractions.Database.Repository.Base;
using Microsoft.EntityFrameworkCore;

namespace CRMService.Application.Abstractions.Database.Repository.OkdeskEntity
{
    public interface IOkdeskTimeEntryRepository :
        IGetItemByIdRepository<TimeEntry, int, DbContext>,
        IGetItemByPredicateRepository<TimeEntry, DbContext>
    {
        Task<List<TimeEntry>> GetLoggedItemsAsync(DateTime dateFrom, DateTime dateTo, long startId, long limit, CancellationToken ct = default);
    }
}
