using CRMService.Domain.Models.OkdeskEntity;

namespace CRMService.Application.Abstractions.Database.Repository.OkdeskEntity
{
    public interface IOkdeskEquipmentRepository
    {
        Task<List<Equipment>> GetSyncItemsAsync(int startId, int limit, CancellationToken ct = default);
    }
}
