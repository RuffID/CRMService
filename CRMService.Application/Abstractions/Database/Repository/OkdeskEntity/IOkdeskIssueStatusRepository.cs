using CRMService.Domain.Models.OkdeskEntity;

namespace CRMService.Application.Abstractions.Database.Repository.OkdeskEntity
{
    public interface IOkdeskIssueStatusRepository
    {
        Task<List<IssueStatus>> GetAllReadOnlyAsync(CancellationToken ct = default);
    }
}
