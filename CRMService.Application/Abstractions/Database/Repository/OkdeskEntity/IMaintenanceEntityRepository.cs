using EFCoreLibrary.Abstractions.Database.Repository.Base;
using CRMService.Domain.Models.OkdeskEntity;

namespace CRMService.Application.Abstractions.Database.Repository.OkdeskEntity
{
    public interface IMaintenanceEntityRepository : IGetItemByIdRepository<MaintenanceEntity, int>, IGetItemByPredicateRepository<MaintenanceEntity>, ICreateItemRepository<MaintenanceEntity>
    {
    }
}


