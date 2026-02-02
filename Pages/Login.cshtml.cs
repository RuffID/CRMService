using CRMService.Interfaces.Repository;
using CRMService.Models.Authorization;
using CRMService.Models.PageModels;
using CRMService.Service.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
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
            if (!await SignIn(UserPage, ModelState, ct))
            {
                if (!ModelState.IsValid)
                    return Page();
                else
                    return Unauthorized();
            }

            return Redirect(returnUrl ?? "/");
        }

        public async Task<bool> SignIn(UserPage userPage, ModelStateDictionary modelState, CancellationToken ct)
        {
            User? user = await unitOfWork.User.GetItemByPredicateAsync(u => u.Login == userPage.Login, asNoTracking: true, include: u => u.Include(u => u.Roles), ct);

            if (user is null || !user.Active || !hash.Verify(userPage.Password, user.Password))
            {
                modelState.AddModelError("", "Wrong login or password.");
                return false;
            }

            List<Claim> claims =
            [
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, user.Name)
            ];

            if (user.Roles.Count > 0)
            {
                foreach (var role in user.Roles)
                    claims.Add(new Claim(ClaimTypes.Role, role.Name));
            }

            ClaimsIdentity claimsIdentity = new(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            ClaimsPrincipal claimsPrincipal = new(claimsIdentity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);

            return true;
        }
    }
}
