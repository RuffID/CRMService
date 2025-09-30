using CRMService.Interfaces.Repository.Base;
using CRMService.Models.OkdeskEntity;

namespace CRMService.Interfaces.Repository.OkdeskEntity
{
    public interface IMaintenanceEntityRepository : IGetItemByIdRepository<MaintenanceEntity, int>, IUpsertItemByIdRepository<MaintenanceEntity, int>, ICreateItemRepository<MaintenanceEntity>
    {
    }
}