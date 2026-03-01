using CRMService.Application.Abstractions.Service;
using CRMService.Contracts.Models.Dto.CrmEntities;
using CRMService.Contracts.Models.Dto.OkdeskEntity;
using CRMService.Application.Models.Report;
using CRMService.Contracts.Models.Request;
using CRMService.Contracts.Models.Responses.Results;
using CRMService.Web.Service.Attributes;
using CRMService.Application.Service.OkdeskEntity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CRMService.Web.Pages
{
    [CookieAuthorize]
    public class ReportModel(IssuePriorityService priorityService, IssueStatusService statusService, IssueTypeService typeService, GroupService groupService, EmployeeService employeeService, IPlanSettingsService planSettingsService, IReportService reportService) : PageModel
    {

        public async Task<IActionResult> OnGetIssuePriorityListAsync(CancellationToken ct)
        {
            ServiceResult<List<PriorityDto>> result = await priorityService.GetIssuePrioritiesAsync(ct);
            return JsonResultMapper.ToJsonResult(result);
        }

        public async Task<IActionResult> OnGetIssueStatusListAsync(CancellationToken ct)
        {
            ServiceResult<List<StatusDto>> result = await statusService.GetIssueStatusesAsync(ct);
            return JsonResultMapper.ToJsonResult(result);
        }

        public async Task<IActionResult> OnGetIssueTypeListAsync(CancellationToken ct)
        {
            ServiceResult<List<TaskTypeDto>> result = await typeService.GetTypes(ct);
            return JsonResultMapper.ToJsonResult(result);
        }

        public async Task<IActionResult> OnGetIssueTypeGroupListAsync(CancellationToken ct)
        {
            ServiceResult<List<IssueTypeGroupDto>> result = await typeService.GetTypeGroups(ct);
            return JsonResultMapper.ToJsonResult(result);
        }

        public async Task<IActionResult> OnGetEmployeeGroupListAsync(CancellationToken ct)
        {
            ServiceResult<List<GroupDto>> result = await groupService.GetGroups(ct);
            return JsonResultMapper.ToJsonResult(result);
        }

        public async Task<IActionResult> OnPostEmployeeListAsync([FromBody] GetEmployeeListRequest request, CancellationToken ct)
        {
            ServiceResult<List<EmployeeDto>> result = await employeeService.GetEmployees(request.GroupIds, ct: ct);
            return JsonResultMapper.ToJsonResult(result);
        }

        public async Task<IActionResult> OnGetPlansAsync(CancellationToken ct)
        {
            ServiceResult<List<PlanDto>> result = await planSettingsService.GetPlans(ct);
            return JsonResultMapper.ToJsonResult(result);
        }

        public async Task<IActionResult> OnGetGeneralSettingsAsync(CancellationToken ct)
        {
            ServiceResult<GeneralSettingsDto> result = await planSettingsService.GetGeneralSettings(ct);
            return JsonResultMapper.ToJsonResult(result);
        }

        public async Task<IActionResult> OnGetPlanColorRulesAsync([FromQuery] Guid planId, CancellationToken ct)
        {
            ServiceResult<List<PlanColorSchemeDto>> result = await planSettingsService.GetPlanColorSchemes(planId, ct);

            return JsonResultMapper.ToJsonResult(result);
        }

        public async Task<IActionResult> OnPostReportAsync([FromBody] ReportRequest request, CancellationToken ct)
        {
            if (request.DateTo.Hour == 0 && request.DateTo.Minute == 0 && request.DateTo.Second == 0)
                request.DateTo = new(request.DateTo.Year, request.DateTo.Month, request.DateTo.Day, hour: 23, minute: 59, second: 59);

            if (request.DateTo <= request.DateFrom)
                return JsonResultMapper.ToJsonResult(ServiceResult<List<ReportInfo>>.Fail(400, "Incorrect period."));

            List<ReportInfo> data = await reportService.GetFullReportOnEmployees(request.DateFrom, request.DateTo, request, ct);
            return JsonResultMapper.ToJsonResult(ServiceResult<List<ReportInfo>>.Ok(data));
        }
    }
}