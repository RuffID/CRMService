using CRMService.Abstractions.Service;
using CRMService.Models.Authorization;
using CRMService.Models.Constants;
using CRMService.Models.Dto.CrmEntities;
using CRMService.Models.Dto.Mappers;
using CRMService.Models.Request;
using CRMService.Models.Responses.Results;
using CRMService.Service.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CRMService.Abstractions.Entity;

namespace CRMService.Pages
{
    [CookieAuthorize]
    [Authorize(Roles = RolesConstants.ADMIN)]
    [LoadUser]
    public class PlanSettingsModel(IPlanSettingsService planSettingsService) : PageModel, IHasCurrentUser
    {
        public User CurrentUser { get; set; } = null!;

        public async Task<IActionResult> OnGetEmployeePlanRowsAsync(CancellationToken ct)
        {
            ServiceResult<List<EmployeePlanRowDto>> result = await planSettingsService.GetEmployeePlanRows(ct);
            return JsonResultMapper.ToJsonResult(result);
        }

        public async Task<IActionResult> OnPostSavePlanSettingsAsync([FromBody] SavePlanSettingsRequest request, CancellationToken ct)
        {
            ServiceResult<bool> result = await planSettingsService.SavePlanSettings(request.Items, ct);
            return JsonResultMapper.ToJsonResult(result);
        }

        public async Task<IActionResult> OnGetPlanColorRulesAsync(CancellationToken ct)
        {
            ServiceResult<List<PlanColorSchemeDto>> result = await planSettingsService.GetPlanColorSchemes(ct);
            return JsonResultMapper.ToJsonResult(result);
        }

        public async Task<IActionResult> OnPostSavePlanColorRulesAsync([FromBody] SavePlanColorSchemesRequest request, CancellationToken ct)
        {
            ServiceResult<bool> result = await planSettingsService.SavePlanColorSchemes(request.Items, ct);
            return JsonResultMapper.ToJsonResult(result);
        }
    }
}
