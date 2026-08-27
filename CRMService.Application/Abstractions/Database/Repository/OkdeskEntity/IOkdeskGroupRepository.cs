using CRMService.Domain.Models.OkdeskEntity;

namespace CRMService.Application.Abstractions.Database.Repository.OkdeskEntity
{
    public interface IOkdeskGroupRepository
    {
        Task<List<Group>> GetAllReadOnlyAsync(CancellationToken ct = default);
    }
}
