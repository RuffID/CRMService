using CRMService.Interfaces.Repository;
using CRMService.Models.Authorization;
using CRMService.Models.PageModels;
using CRMService.Service.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Security.Claims;

namespace CRMService.PageHandlers
{
    public class SignInHandler(IUnitOfWork unitOfWork, Hasher hasher, Hasher hash, IHttpContextAccessor context)
    {
        public async Task<bool> SignIn(UserPage userPage, ModelStateDictionary modelState, CancellationToken ct)
        {
            User? user = await unitOfWork.User.GetItemByPredicate(u => u.Login == userPage.Login, asNoTracking: true, ct);

            if (user is null || !user.Active || !hash.Verify(hasher.Hash(userPage.Password), user.Password))
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

            ClaimsIdentity claimsIdentity = new (claims, CookieAuthenticationDefaults.AuthenticationScheme);
            ClaimsPrincipal claimsPrincipal = new (claimsIdentity);

            HttpContext? httpContext = context.HttpContext;

            if (httpContext == null)
                return false;

            await httpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);            

            return true;
        }
    }
}
