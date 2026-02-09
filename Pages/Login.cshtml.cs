using CRMService.Abstractions.Database.Repository;
using CRMService.Models.Authorization;
using CRMService.Models.Request;
using CRMService.Service.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CRMService.Pages
{
    public class LoginModel(IUnitOfWork unitOfWork, Hasher hash) : PageModel
    {
        [BindProperty]
        public UserPage UserPage { get; set; } = new();

        public async Task<IActionResult> OnPostAsync(string? returnUrl, CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return Page();

            User? user = await unitOfWork.User.GetItemByPredicateAsync(u => u.Login.ToLower() == UserPage.Login.ToLower(), asNoTracking: true, include: u => u.Include(u => u.Roles), ct);

            if (user is null || !user.Active || !hash.Verify(UserPage.Password, user.Password))
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

            ClaimsIdentity identity = new(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            ClaimsPrincipal principal = new(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToPage("/Index");
        }
    }
}
