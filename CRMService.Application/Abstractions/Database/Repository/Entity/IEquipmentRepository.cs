using CRMService.Application.Abstractions.Database.Repository.Base;
using CRMService.Contracts.Models.Request;
using CRMService.Domain.Models.OkdeskEntity;

namespace CRMService.Application.Abstractions.Database.Repository.Entity
{
    public interface IEquipmentRepository :
        IGetItemByIdRepository<Equipment, int>,
        ICreateItemRepository<Equipment>
    {
        Task<Equipment?> GetWithParametersReadOnlyAsync(int id, CancellationToken ct = default);
        Task<Equipment?> GetDetailsReadOnlyAsync(int id, CancellationToken ct = default);
        Task<List<Equipment>> GetByMaintenanceEntityWithParametersReadOnlyAsync(int maintenanceEntityId, CancellationToken ct = default);
        Task<List<Equipment>> GetByCompanyWithParametersReadOnlyAsync(int companyId, CancellationToken ct = default);
        Task<int> GetCountByFilterAsync(EquipmentListRequest request, int? maxCount, CancellationToken ct);

        Task<List<Equipment>> GetPageByFilterAsync(EquipmentListRequest request, int skip, int take, CancellationToken ct);
    }
}
