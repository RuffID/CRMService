using Microsoft.EntityFrameworkCore;
using EFCoreLibrary.Abstractions.Database.Repository.Base;
using CRMService.Contracts.Models.Request;
using CRMService.Domain.Models.OkdeskEntity;

namespace CRMService.Application.Abstractions.Database.Repository.Entity
{
    public interface IEquipmentRepository :
        IGetItemByIdRepository<Equipment, int, DbContext>,
        IGetItemByPredicateRepository<Equipment, DbContext>,
        ICreateItemRepository<Equipment, DbContext>
    {
        Task<int> GetCountByFilterAsync(EquipmentListRequest request, int? maxCount, CancellationToken ct);

        Task<List<Equipment>> GetPageByFilterAsync(EquipmentListRequest request, int skip, int take, CancellationToken ct);
    }
}
