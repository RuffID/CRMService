using CRMService.Models.Dto.CrmEntities;
using CRMService.Models.Responses.Results;

namespace CRMService.Abstractions.Service
{
    public interface IPlanSettingsService
    {
        Task<ServiceResult<List<EmployeePlanRowDto>>> GetEmployeePlanRows(CancellationToken ct = default);
        Task<ServiceResult<bool>> SavePlanSettings(List<PlanSettingDto> items, CancellationToken ct = default);

        Task<ServiceResult<List<PlanColorSchemeDto>>> GetPlanColorSchemes(CancellationToken ct = default);
        Task<ServiceResult<bool>> SavePlanColorSchemes(List<PlanColorSchemeDto> items, CancellationToken ct = default);
    }
}
