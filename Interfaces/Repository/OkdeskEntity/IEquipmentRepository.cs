using CRMService.Interfaces.Repository.Base;
using CRMService.Models.Dto.OkdeskEntity;
using CRMService.Models.OkdeskEntity;

namespace CRMService.Interfaces.Repository.OkdeskEntity
{
    public interface IEquipmentRepository : IGetItemByIdRepository<Equipment, int>, IGetItemByPredicateRepository<Equipment>, ICreateItemRepository<Equipment>
    {
    }
}