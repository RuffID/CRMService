using CRMService.Abstractions.Database.Repository.Base;
using CRMService.Models.Dto.OkdeskEntity;
using CRMService.Models.OkdeskEntity;

namespace CRMService.Abstractions.Database.Repository.OkdeskEntity
{
    public interface IEquipmentRepository : IGetItemByIdRepository<Equipment, int>, IGetItemByPredicateRepository<Equipment>, ICreateItemRepository<Equipment>
    {
    }
}