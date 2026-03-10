using CRMService.Web.Service.Attributes;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CRMService.Web.Pages
{
    [CookieAuthorize]
    public class IndexModel : PageModel
    {
    }
}