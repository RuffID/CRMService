using CRMService.Application.Abstractions.Service;
using CRMService.Domain.Models.Constants;
using CRMService.Contracts.Models.Dto.CrmEntities;
using CRMService.Contracts.Models.Request;
using CRMService.Contracts.Models.Responses.Results;
using CRMService.Web.Service.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CRMService.Web.Pages
{
    [CookieAuthorize]
    [Authorize(Roles = RolesConstants.ADMIN)]
    public class PlanSettingsModel(IPlanSettingsService planSettingsService) : PageModel
    {
        public async Task<IActionResult> OnGetPlansAsync(CancellationToken ct)
        {
            ServiceResult<List<PlanDto>> result = await planSettingsService.GetPlans(ct);
            return JsonResultMapper.ToJsonResult(result);
        }

        public async Task<IActionResult> OnPostSavePlansAsync([FromBody] SavePlansRequest request, CancellationToken ct)
        {
            ServiceResult<bool> result = await planSettingsService.SavePlans(request.Items, ct);
            return JsonResultMapper.ToJsonResult(result);
        }

        public async Task<IActionResult> OnGetGeneralSettingsAsync(CancellationToken ct)
        {
            ServiceResult<GeneralSettingsDto> result = await planSettingsService.GetGeneralSettings(ct);
            return JsonResultMapper.ToJsonResult(result);
        }

        public async Task<IActionResult> OnPostSaveGeneralSettingsAsync([FromBody] SaveGeneralSettingsRequest request, CancellationToken ct)
        {
            ServiceResult<bool> result = await planSettingsService.SaveGeneralSettings(request.Item, ct);
            return JsonResultMapper.ToJsonResult(result);
        }

        public async Task<IActionResult> OnGetEmployeePlanRowsAsync([FromQuery] Guid planId, CancellationToken ct)
        {
            ServiceResult<List<EmployeePlanRowDto>> result = await planSettingsService.GetEmployeePlanRows(planId, ct);
            return JsonResultMapper.ToJsonResult(result);
        }

        public async Task<IActionResult> OnPostSavePlanSettingsAsync([FromBody] SavePlanSettingsRequest request, CancellationToken ct)
        {
            ServiceResult<bool> result = await planSettingsService.SavePlanSettings(request.Items, ct);
            return JsonResultMapper.ToJsonResult(result);
        }

        public async Task<IActionResult> OnGetPlanColorRulesAsync([FromQuery] Guid planId, CancellationToken ct)
        {
            ServiceResult<List<PlanColorSchemeDto>> result = await planSettingsService.GetPlanColorSchemes(planId, ct);
            return JsonResultMapper.ToJsonResult(result);
        }

        public async Task<IActionResult> OnPostSavePlanColorRulesAsync([FromBody] SavePlanColorSchemesRequest request, CancellationToken ct)
        {
            ServiceResult<bool> result = await planSettingsService.SavePlanColorSchemes(request.PlanId, request.Items, ct);
            return JsonResultMapper.ToJsonResult(result);
        }
    }
}