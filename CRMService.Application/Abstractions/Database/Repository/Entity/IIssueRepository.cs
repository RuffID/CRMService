using CRMService.Application.Abstractions.Database.Repository.Base;
using CRMService.Contracts.Models.Request;
using CRMService.Domain.Models.OkdeskEntity;

namespace CRMService.Application.Abstractions.Database.Repository.Entity
{
    public interface IIssueRepository :
        IGetItemByIdRepository<Issue, int>,
        ICreateItemRepository<Issue>
    {
        Task<Issue?> GetDetailsReadOnlyAsync(int id, CancellationToken ct = default);
        Task<List<Issue>> GetFromIdReadOnlyAsync(int startId, int limit, CancellationToken ct = default);
        Task<List<Issue>> GetUpdatedLocalReadOnlyAsync(DateTime dateFrom, DateTime dateTo, CancellationToken ct = default);
        Task<List<Issue>> GetByIdsAsync(IReadOnlyCollection<int> ids, CancellationToken ct = default);
        Task<int> GetCountByFilterAsync(IssueListRequest request, int? maxCount, CancellationToken ct);
        Task<List<Issue>> GetPageByFilterAsync(IssueListRequest request, int skip, int take, CancellationToken ct);
        Task<List<int>> GetCurrentIssueIdsAsync(CancellationToken ct);
    }
}
