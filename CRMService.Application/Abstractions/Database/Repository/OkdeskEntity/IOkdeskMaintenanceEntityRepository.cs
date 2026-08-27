using CRMService.Domain.Models.OkdeskEntity;

namespace CRMService.Application.Abstractions.Database.Repository.OkdeskEntity
{
    public interface IOkdeskMaintenanceEntityRepository
    {
        Task<List<MaintenanceEntity>> GetAllWithCompanyReadOnlyAsync(CancellationToken ct = default);
    }
}
