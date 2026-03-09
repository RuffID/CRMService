using CRMService.Application.Abstractions.Entity;
using CRMService.Application.Abstractions.Service;
using CRMService.Application.Service.OkdeskEntity;
using CRMService.Contracts.Models.Dto.OkdeskEntity;
using CRMService.Contracts.Models.Dto.Report;
using CRMService.Contracts.Models.Request;
using CRMService.Contracts.Models.Responses.Results;
using CRMService.Domain.Models.Authorization;
using CRMService.Web.Service.Attributes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CRMService.Web.Pages
{
    [CookieAuthorize]
    public class IndexModel(GroupService groupService, EmployeeService employeeService, IReportService reportService) : PageModel
    {
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

        public async Task<IActionResult> OnPostTimeChartAsync([FromBody] TimeChartRequest request, CancellationToken ct)
        {
            if (request.DateTo.Hour == 0 && request.DateTo.Minute == 0 && request.DateTo.Second == 0)
                request.DateTo = new DateTime(request.DateTo.Year, request.DateTo.Month, request.DateTo.Day, 23, 59, 59);

            if (request.DateTo <= request.DateFrom)
                return JsonResultMapper.ToJsonResult(ServiceResult<TimeChartDto>.Fail(400, "Некорректный период."));

            TimeChartDto data = await reportService.GetSpentTimeChart(request, ct);
            return JsonResultMapper.ToJsonResult(ServiceResult<TimeChartDto>.Ok(data));
        }
    }
}