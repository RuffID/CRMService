using CRMService.Domain.Models.OkdeskEntity;
using EFCoreLibrary.Abstractions.Database.Repository.Base;
using Microsoft.EntityFrameworkCore;

namespace CRMService.Application.Abstractions.Database.Repository.OkdeskEntity
{
    public interface IOkdeskEquipmentRepository :
        IGetItemByIdRepository<Equipment, int, DbContext>,
        IGetItemByPredicateRepository<Equipment, DbContext>
    {
        Task<List<Equipment>> GetSyncItemsAsync(int startId, int limit, CancellationToken ct = default);
    }
}
