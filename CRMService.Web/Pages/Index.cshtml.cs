using CRMService.Domain.Models.Authorization;
using CRMService.Web.Service.Attributes;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CRMService.Application.Abstractions.Entity;

namespace CRMService.Web.Pages
{
    [CookieAuthorize]
    [LoadUser]
    public class IndexModel : PageModel, IHasCurrentUser
    {
        private readonly ILogger<IndexModel> _logger;

        public User CurrentUser { get; set; } = null!;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {

        }
    }
}





