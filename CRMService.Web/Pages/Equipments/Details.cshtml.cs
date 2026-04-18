using CRMService.Domain.Models.Constants;
using CRMService.Web.Service.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CRMService.Web.Pages.Equipments
{
    [CookieAuthorize]
    [Authorize(Roles = RolesConstants.ADMIN)]
    public class DetailsModel : PageModel
    {
        public string BackUrl { get; private set; } = "/equipments?page=1";

        public void OnGet(int id, [FromQuery(Name = "page")] int? page)
        {
            int listPage = page.HasValue && page.Value > 0 ? page.Value : 1;
            BackUrl = $"/equipments?page={listPage}";
        }
    }
}
