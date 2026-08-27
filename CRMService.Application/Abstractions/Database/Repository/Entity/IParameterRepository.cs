using CRMService.Application.Abstractions.Database.Repository.Base;
using CRMService.Domain.Models.OkdeskEntity;

namespace CRMService.Application.Abstractions.Database.Repository.Entity
{
    public interface IParameterRepository :
        ICreateItemRepository<EquipmentParameter>
    {
        Task<List<EquipmentParameter>> GetByEquipmentIdAsync(int equipmentId, CancellationToken ct = default);
    }
}
