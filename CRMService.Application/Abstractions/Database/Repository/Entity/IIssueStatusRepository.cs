using CRMService.Application.Abstractions.Database.Repository.Base;
using CRMService.Domain.Models.OkdeskEntity;

namespace CRMService.Application.Abstractions.Database.Repository.Entity
{
    public interface IIssueStatusRepository :
        IGetItemByIdRepository<IssueStatus, int>,
        IGetItemsRepository<IssueStatus>,
        ICreateItemRepository<IssueStatus>
    {
        Task<IssueStatus?> GetByCodeAsync(string code, CancellationToken ct = default);
        Task<IssueStatus?> GetByCodeReadOnlyAsync(string code, CancellationToken ct = default);
        Task<List<IssueStatus>> SearchReadOnlyAsync(string? search, CancellationToken ct = default);
        Task<List<IssueStatus>> GetByCodesReadOnlyAsync(IReadOnlyCollection<string> codes, CancellationToken ct = default);
    }
}
