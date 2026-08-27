using CRMService.Domain.Models.OkdeskEntity;

namespace CRMService.Application.Abstractions.Database.Repository.OkdeskEntity
{
    public interface IOkdeskIssueRepository
    {
        Task<List<Issue>> GetUpdatedItemsAsync(DateTime dateFrom, DateTime dateTo, int startId, int limit, CancellationToken ct = default);
    }
}
