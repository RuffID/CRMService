using CRMService.Interfaces.Repository.Base;
using CRMService.Models.Entity;

namespace CRMService.Interfaces.Repository.Entity
{
    public interface IMaintenanceEntityRepository : IGetItemByIdRepository<MaintenanceEntity, int>, IUpsertItemByIdRepository<MaintenanceEntity, int>, ICreateItemRepository<MaintenanceEntity>
    {
    }
}