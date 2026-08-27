using CRMService.Domain.Models.CrmEntities;

namespace CRMService.Infrastructure.DataBase.Repository.CrmEntity;

public partial class GeneralSettingsRepository
{
    public Task<GeneralSettings?> GetItemByIdAsync(Guid id, CancellationToken ct = default) => getItemById.GetItemByIdAsync(id, ct: ct);
    public Task<GeneralSettings?> GetItemByIdReadOnlyAsync(Guid id, CancellationToken ct = default) => getItemById.GetItemByIdAsync(id, true, ct: ct);
    public Task<List<GeneralSettings>> GetItemsAsync(CancellationToken ct = default) => getItemByPredicate.GetItemsByPredicateAsync(ct: ct);
    public Task<List<GeneralSettings>> GetItemsReadOnlyAsync(CancellationToken ct = default) => getItemByPredicate.GetItemsByPredicateAsync(asNoTracking: true, ct: ct);
}

public partial class PlanRepository
{
    public Task<Plan?> GetItemByIdAsync(Guid id, CancellationToken ct = default) => getItemById.GetItemByIdAsync(id, ct: ct);
    public Task<Plan?> GetItemByIdReadOnlyAsync(Guid id, CancellationToken ct = default) => getItemById.GetItemByIdAsync(id, true, ct: ct);
    public Task<List<Plan>> GetItemsAsync(CancellationToken ct = default) => getItemByPredicate.GetItemsByPredicateAsync(ct: ct);
    public Task<List<Plan>> GetItemsReadOnlyAsync(CancellationToken ct = default) => getItemByPredicate.GetItemsByPredicateAsync(asNoTracking: true, ct: ct);
    public Task<List<Plan>> GetByIdsReadOnlyAsync(IReadOnlyCollection<Guid> ids, CancellationToken ct = default) => getItemByPredicate.GetItemsByPredicateAsync(x => ids.Contains(x.Id), asNoTracking: true, ct: ct);
}

public partial class PlanColorSchemeRepository
{
    public Task<PlanColorScheme?> GetItemByIdAsync(Guid id, CancellationToken ct = default) => getItemById.GetItemByIdAsync(id, ct: ct);
    public Task<PlanColorScheme?> GetItemByIdReadOnlyAsync(Guid id, CancellationToken ct = default) => getItemById.GetItemByIdAsync(id, true, ct: ct);
    public Task<List<PlanColorScheme>> GetByPlanIdAsync(Guid planId, CancellationToken ct = default) => getItemByPredicate.GetItemsByPredicateAsync(x => x.PlanId == planId, ct: ct);
    public Task<List<PlanColorScheme>> GetByPlanIdReadOnlyAsync(Guid planId, CancellationToken ct = default) => getItemByPredicate.GetItemsByPredicateAsync(x => x.PlanId == planId, asNoTracking: true, ct: ct);
}

public partial class PlanSettingRepository
{
    public Task<List<PlanSetting>> GetByPlanAndEmployeesReadOnlyAsync(Guid planId, IReadOnlyCollection<int> employeeIds, CancellationToken ct = default) => getItemByPredicate.GetItemsByPredicateAsync(x => x.PlanId == planId && employeeIds.Contains(x.EmployeeId), asNoTracking: true, ct: ct);
    public Task<List<PlanSetting>> GetByPlansAndEmployeesAsync(IReadOnlyCollection<Guid> planIds, IReadOnlyCollection<int> employeeIds, CancellationToken ct = default) => getItemByPredicate.GetItemsByPredicateAsync(x => planIds.Contains(x.PlanId) && employeeIds.Contains(x.EmployeeId), ct: ct);
}
