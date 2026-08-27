using CRMService.Application.Abstractions.Database.Repository.Base;
using CRMService.Domain.Models.CrmEntities;

namespace CRMService.Application.Abstractions.Database.Repository.CrmEntity
{
    public interface IPlanColorSchemeRepository :
        IGetItemByIdRepository<PlanColorScheme, Guid>,
        ICreateItemRepository<PlanColorScheme>,
        IDeleteItemRepository<PlanColorScheme>
    {
        Task<List<PlanColorScheme>> GetByPlanIdAsync(Guid planId, CancellationToken ct = default);
        Task<List<PlanColorScheme>> GetByPlanIdReadOnlyAsync(Guid planId, CancellationToken ct = default);
    }
}
