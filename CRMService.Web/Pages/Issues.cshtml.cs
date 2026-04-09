using CRMService.Application.Service.OkdeskEntity;
using CRMService.Contracts.Models.Request;
using CRMService.Domain.Models.Constants;
using CRMService.Web.Core.Mappers;
using CRMService.Web.Service.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CRMService.Web.Pages
{
    [CookieAuthorize]
    [Authorize(Roles = RolesConstants.ADMIN)]
    public class IssuesModel(
        IssueService issueService,
        IssueStatusService issueStatusService,
        IssueTypeService issueTypeService,
        GroupService groupService,
        EmployeeService employeeService,
        CompanyService companyService) : PageModel
    {
        public async Task<IActionResult> OnGetListAsync([FromQuery] IssueListRequest request, CancellationToken ct)
        {
            return JsonResultMapper.ToJsonResult(await issueService.GetIssueListPageAsync(request, ct));
        }

        public async Task<IActionResult> OnGetExactCountAsync([FromQuery] IssueListRequest request, CancellationToken ct)
        {
            return JsonResultMapper.ToJsonResult(await issueService.GetIssueExactCountAsync(request, ct));
        }

        public async Task<IActionResult> OnGetAssigneeLookupAsync([FromQuery] LookupListRequest request, CancellationToken ct)
        {
            return JsonResultMapper.ToJsonResult(await employeeService.GetEmployeeLookupAsync(request, ct));
        }

        public async Task<IActionResult> OnGetAuthorLookupAsync([FromQuery] LookupListRequest request, CancellationToken ct)
        {
            return JsonResultMapper.ToJsonResult(await employeeService.GetEmployeeLookupAsync(request, ct));
        }

        public async Task<IActionResult> OnGetTypeLookupAsync([FromQuery] LookupListRequest request, CancellationToken ct)
        {
            return JsonResultMapper.ToJsonResult(await issueTypeService.GetTypeLookupAsync(request, ct));
        }

        public async Task<IActionResult> OnGetStatusLookupAsync([FromQuery] LookupListRequest request, CancellationToken ct)
        {
            return JsonResultMapper.ToJsonResult(await issueStatusService.GetIssueStatusLookupAsync(request, ct));
        }

        public async Task<IActionResult> OnGetCompanyLookupAsync([FromQuery] LookupListRequest request, CancellationToken ct)
        {
            return JsonResultMapper.ToJsonResult(await companyService.GetCompanyLookupAsync(request, ct));
        }

        public async Task<IActionResult> OnGetGroupLookupAsync([FromQuery] LookupListRequest request, CancellationToken ct)
        {
            return JsonResultMapper.ToJsonResult(await groupService.GetGroupLookupAsync(request, ct));
        }
    }
}
