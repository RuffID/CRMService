using CRMService.Interfaces.Repository.Base;
using CRMService.Models.Dto.Entity;
using CRMService.Models.Entity;

namespace CRMService.Interfaces.Repository.Entity
{
    public interface IParameterRepository : IGetItemByPredicateRepository<EquipmentParameter>, IUpsertItemByPredicateRepository<EquipmentParameter>, ICreateItemRepository<EquipmentParameter>
    {
    }
}
