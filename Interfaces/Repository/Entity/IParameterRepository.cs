using CRMService.Interfaces.Repository.Base;
using CRMService.Interfaces.Repository.Extended;
using CRMService.Models.Dto.Entity;
using CRMService.Models.Entity;

namespace CRMService.Interfaces.Repository.Entity
{
    public interface IParameterRepository : IGetItemByPredicateRepository<EquipmentParameter>, IUpsertByPredicateRepository<EquipmentParameter>, ICreateItemRepository<EquipmentParameter>
    {
        Task<List<EquipmentParameter>> GetParameterByEquipmentId(int equipmentId, CancellationToken ct);
        Task<EquipmentParameter?> GetParameterByEquipmentAndKindParameterId(int equipmentId, int kindParameterId, CancellationToken ct);
    }
}
