using CRMService.Application.Abstractions.Database.Repository.Base;
using CRMService.Domain.Models.CrmEntities;

namespace CRMService.Application.Abstractions.Database.Repository.CrmEntity
{
    public interface IPlanSettingRepository :
        ICreateItemRepository<PlanSetting>,
        IDeleteItemRepository<PlanSetting>
    {
        Task<List<PlanSetting>> GetByPlanAndEmployeesReadOnlyAsync(Guid planId, IReadOnlyCollection<int> employeeIds, CancellationToken ct = default);
        Task<List<PlanSetting>> GetByPlansAndEmployeesAsync(IReadOnlyCollection<Guid> planIds, IReadOnlyCollection<int> employeeIds, CancellationToken ct = default);
    }
}
