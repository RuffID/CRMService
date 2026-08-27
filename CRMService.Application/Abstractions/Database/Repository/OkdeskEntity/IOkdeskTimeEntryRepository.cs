using CRMService.Domain.Models.OkdeskEntity;

namespace CRMService.Application.Abstractions.Database.Repository.OkdeskEntity
{
    public interface IOkdeskTimeEntryRepository
    {
        Task<List<TimeEntry>> GetLoggedItemsAsync(DateTime dateFrom, DateTime dateTo, long startId, long limit, CancellationToken ct = default);
    }
}
