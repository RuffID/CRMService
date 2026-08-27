using CRMService.Domain.Models.OkdeskEntity;

namespace CRMService.Application.Abstractions.Database.Repository.OkdeskEntity
{
    public interface IOkdeskIssueTypeRepository
    {
        Task<List<IssueType>> GetAllReadOnlyAsync(CancellationToken ct = default);
    }
}
