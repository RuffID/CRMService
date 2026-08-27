using CRMService.Domain.Models.OkdeskEntity;

namespace CRMService.Application.Abstractions.Database.Repository.OkdeskEntity
{
    public interface IOkdeskIssueTypeGroupRepository
    {
        Task<List<IssueTypeGroup>> GetAllReadOnlyAsync(CancellationToken ct = default);
    }
}
