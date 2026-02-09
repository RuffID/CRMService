using CRMService.Abstractions.Database.Repository.Base;
using CRMService.Models.OkdeskEntity;

namespace CRMService.Abstractions.Database.Repository.OkdeskEntity
{
    public interface IMaintenanceEntityRepository : IGetItemByIdRepository<MaintenanceEntity, int>, IGetItemByPredicateRepository<MaintenanceEntity>, ICreateItemRepository<MaintenanceEntity>
    {
    }
}