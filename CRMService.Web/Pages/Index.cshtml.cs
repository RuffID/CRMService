using CRMService.Domain.Models.Authorization;
using CRMService.Web.Service.Attributes;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CRMService.Application.Abstractions.Entity;

namespace CRMService.Web.Pages
{
    [CookieAuthorize]
    [LoadUser]
    public class IndexModel(ILogger<IndexModel> logger) : PageModel, IHasCurrentUser
    {
        public User CurrentUser { get; set; } = null!;


    }
}