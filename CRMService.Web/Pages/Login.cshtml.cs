using CRMService.Application.Abstractions.Database.Repository;
using CRMService.Domain.Models.Authorization;
using CRMService.Contracts.Models.Request;
using CRMService.Application.Service.Authorization;
using CRMService.Web.Service.Attributes;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using ApplicationAuthenticationService = CRMService.Application.Service.Authorization.AuthenticationService;

namespace CRMService.Web.Pages
{
    [CookieAuthorize]
    public class LoginModel(ApplicationAuthenticationService authenticationService) : PageModel
    {
        [BindProperty]
        public UserPageRequest UserPage { get; set; } = new();

        public IActionResult OnGet()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToPage("/Index");

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl, CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return Page();

            User? user = await authenticationService.AuthenticateAsync(UserPage.Login, UserPage.Password, ignoreLoginCase: true, ct);

            if (user is null)
            {
                ModelState.AddModelError(string.Empty, "Неправильный логин или пароль.");
                return Page();
            }

            List<Claim> claims =
            [
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, user.Name)
            ];

            if (user.Roles.Count > 0)
            {
                foreach (CrmRole role in user.Roles)
                    claims.Add(new Claim(ClaimTypes.Role, role.Name));
            }

            AuthenticationProperties props = new() { IsPersistent = true };

            ClaimsIdentity identity = new(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            ClaimsPrincipal principal = new(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, props);

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToPage("/Index");
        }
    }
}
