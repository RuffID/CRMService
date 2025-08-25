using CRMService.Interfaces.Repository;
using CRMService.Models.PageModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Security.Claims;

namespace CRMService.PageHandlers
{
    public class SignInHandler(IUnitOfWork unitOfWork, CryptographyService crypto, IHttpContextAccessor context)
    {
        public async Task<bool> SignIn(UserPage? user, ModelStateDictionary modelState)
        {
            if (user == null || string.IsNullOrEmpty(user.Login) || string.IsNullOrEmpty(user.Password))
            {
                modelState.AddModelError($"{nameof(UserPage)}.{nameof(UserPage.Login)}", "Логин или пароль не могут быть пустыми.");
                return false;
            }

            User? userFromDb = await unitOfWork.User.GetUserByLogin(user.Login);

            if (userFromDb is null || !userFromDb.Active || userFromDb.Password != crypto.GetUserSaltPass(user.Login, user.Password))
            {
                modelState.AddModelError($"{nameof(UserPage)}.{nameof(user.Login)}", "Неправильный логин или пароль.");
                return false;
            }

            List<Claim> claims = new()
            {
                new(ClaimTypes.NameIdentifier, userFromDb.Id.ToString()),
                new(ClaimTypes.Name, userFromDb.Name ?? "unknown"),
                new(ClaimsIdentity.DefaultRoleClaimType, userFromDb.Role.ToString() ?? "unknown"),
                new("LoginDate", DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm")),
                new(nameof(User.OrganizationId), userFromDb.OrganizationId.ToString()),
                new(ClaimTypes.Role, userFromDb.Role.ToString())
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            HttpContext? httpContext = context.HttpContext;

            if (httpContext == null)
                return false;

            await httpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);            

            return true;
        }
    }
}
