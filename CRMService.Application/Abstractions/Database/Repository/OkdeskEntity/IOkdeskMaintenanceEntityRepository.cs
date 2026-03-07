using CRMService.Domain.Models.OkdeskEntity;
using EFCoreLibrary.Abstractions.Database.Repository.Base;
using Microsoft.EntityFrameworkCore;

namespace CRMService.Application.Abstractions.Database.Repository.OkdeskEntity
{
    public interface IOkdeskMaintenanceEntityRepository :
        IGetItemByIdRepository<MaintenanceEntity, int, DbContext>,
        IGetItemByPredicateRepository<MaintenanceEntity, DbContext>
    {
    }
}
