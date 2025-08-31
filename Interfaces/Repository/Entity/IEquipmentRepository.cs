using CRMService.Interfaces.Repository.Base;
using CRMService.Models.Dto.Entity;
using CRMService.Models.Entity;

namespace CRMService.Interfaces.Repository.Entity
{
    public interface IEquipmentRepository : IGetItemByIdRepository<Equipment, int>, IUpsertItemByIdRepository<Equipment, int>, ICreateItemRepository<Equipment>
    {
    }
}