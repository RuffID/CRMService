using Microsoft.EntityFrameworkCore;
using EFCoreLibrary.Abstractions.Database.Repository.Base;
using CRMService.Domain.Models.OkdeskEntity;

namespace CRMService.Application.Abstractions.Database.Repository.OkdeskEntity
{
    public interface IMaintenanceEntityRepository :
        IGetItemByIdRepository<MaintenanceEntity, int, DbContext>,
        IGetItemByPredicateRepository<MaintenanceEntity, DbContext>,
        ICreateItemRepository<MaintenanceEntity, DbContext>
    {
    }
}