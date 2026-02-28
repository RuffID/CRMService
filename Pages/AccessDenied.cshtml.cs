using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CRMService.Pages
{
    public class AccessDeniedModel : PageModel
    {
        public void OnGet()
        {
            Response.StatusCode = StatusCodes.Status404NotFound;
        }
    }
}
