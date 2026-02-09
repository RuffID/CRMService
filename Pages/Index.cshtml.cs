using CRMService.Abstractions.Entity;
using CRMService.Models.Authorization;
using CRMService.Service.Attributes;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CRMService.Pages
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
