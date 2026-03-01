using CRMService.Models.Dto.CrmEntities;
using CRMService.Models.Responses.Results;

namespace CRMService.Abstractions.Service
{
    public interface IPlanSettingsService
    {
        Task<ServiceResult<List<PlanDto>>> GetPlans(CancellationToken ct = default);
        Task<ServiceResult<bool>> SavePlans(List<PlanDto> items, CancellationToken ct = default);

        Task<ServiceResult<GeneralSettingsDto>> GetGeneralSettings(CancellationToken ct = default);
        Task<ServiceResult<bool>> SaveGeneralSettings(GeneralSettingsDto item, CancellationToken ct = default);

        Task<ServiceResult<List<EmployeePlanRowDto>>> GetEmployeePlanRows(Guid planId, CancellationToken ct = default);
        Task<ServiceResult<bool>> SavePlanSettings(List<PlanSettingDto> items, CancellationToken ct = default);

        Task<ServiceResult<List<PlanColorSchemeDto>>> GetPlanColorSchemes(Guid planId, CancellationToken ct = default);
        Task<ServiceResult<bool>> SavePlanColorSchemes(Guid planId, List<PlanColorSchemeDto> items, CancellationToken ct = default);
    }
}