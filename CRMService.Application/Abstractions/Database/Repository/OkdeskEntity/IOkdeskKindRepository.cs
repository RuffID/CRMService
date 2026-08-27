using CRMService.Domain.Models.OkdeskEntity;

namespace CRMService.Application.Abstractions.Database.Repository.OkdeskEntity
{
    public interface IOkdeskKindRepository
    {
        Task<List<Kind>> GetAllReadOnlyAsync(CancellationToken ct = default);
    }
}
