using CRMService.Domain.Models.OkdeskEntity;

namespace CRMService.Application.Abstractions.Database.Repository.OkdeskEntity
{
    public interface IOkdeskIssuePriorityRepository
    {
        Task<List<IssuePriority>> GetAllReadOnlyAsync(CancellationToken ct = default);
    }
}
