using CRMService.Application.Service.OkdeskEntity;
using CRMService.Domain.Models.Constants;
using CRMService.Web.Core.Mappers;
using CRMService.Web.Service.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CRMService.Web.Pages.Issues
{
    [CookieAuthorize]
    [Authorize(Roles = RolesConstants.ADMIN)]
    public class DetailsModel(IssueService issueService) : PageModel
    {
        public string BackUrl { get; private set; } = "/issues?page=1";
        public int IssueId { get; private set; }

        public void OnGet(int id, [FromQuery(Name = "page")] int? page)
        {
            IssueId = id;
            int listPage = page.HasValue && page.Value > 0 ? page.Value : 1;
            BackUrl = $"/issues?page={listPage}";
        }

        public async Task<IActionResult> OnGetDetailsAsync(int id, CancellationToken ct)
        {
            return JsonResultMapper.ToJsonResult(await issueService.GetIssueDetailsAsync(id, ct));
        }
    }
}
