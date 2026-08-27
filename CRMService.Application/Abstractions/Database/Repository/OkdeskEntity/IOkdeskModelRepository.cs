using CRMService.Domain.Models.OkdeskEntity;

namespace CRMService.Application.Abstractions.Database.Repository.OkdeskEntity
{
    public interface IOkdeskModelRepository
    {
        Task<List<Model>> GetAllReadOnlyAsync(CancellationToken ct = default);
    }
}
