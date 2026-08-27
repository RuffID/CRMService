using CRMService.Application.Abstractions.Database.Repository.Base;
using CRMService.Domain.Models.CrmEntities;

namespace CRMService.Application.Abstractions.Database.Repository.CrmEntity
{
    public interface IPlanRepository :
        IGetItemByIdRepository<Plan, Guid>,
        IGetItemsRepository<Plan>,
        ICreateItemRepository<Plan>,
        IDeleteItemRepository<Plan>
    {
        Task<List<Plan>> GetByIdsReadOnlyAsync(IReadOnlyCollection<Guid> ids, CancellationToken ct = default);
    }
}
