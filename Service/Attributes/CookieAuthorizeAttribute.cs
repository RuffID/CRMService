using CRMService.Abstractions.Database.Repository;
using CRMService.Models.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CRMService.Service.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class CookieAuthorizeAttribute(string cookieName = ".CRMService.Cookies") : Attribute, IAsyncPageFilter, IOrderedFilter
    {
        private const string CURRENT_USER_ITEM_KEY = "CurrentUser";
        public int Order { get; set; } = 0;

        public async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
        {
            if (context.HandlerInstance is not PageModel page)
                throw new InvalidOperationException($"{nameof(CookieAuthorizeAttribute)} can only be used on Razor Page models.");

            HttpContext httpContext = context.HttpContext;
            HttpRequest request = httpContext.Request;

            string path = request.Path.ToString().ToLowerInvariant();
            bool isAuthPage = path == "/login";

            if (!request.Cookies.TryGetValue(cookieName, out string? cookieValue) || string.IsNullOrWhiteSpace(cookieValue))
            {
                if (!isAuthPage)
                {
                    httpContext.Response.Redirect("/login");
                    return;
                }

                await next();
                return;
            }

            string? userIdStr = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!Guid.TryParse(userIdStr, out Guid userId))
            {
                httpContext.Response.Redirect("/");
                return;
            }

            IUnitOfWork unitOfWork = httpContext.RequestServices.GetRequiredService<IUnitOfWork>();
            User? user = await unitOfWork.User.GetItemByIdAsync(userId, asNoTracking: true, u => u.Include(u => u.Roles), httpContext.RequestAborted);

            if (user is null || !user.Active)
            {
                if (!isAuthPage)
                {
                    httpContext.Response.Redirect("/Login");
                    return;
                }

                await next();
                return;
            }

            if (isAuthPage)
            {
                httpContext.Response.Redirect("/Index");
                return;
            }

            httpContext.Items[CURRENT_USER_ITEM_KEY] = user;

            await next();
        }

        public Task OnPageHandlerSelectionAsync(PageHandlerSelectedContext context) => Task.CompletedTask;
    }
}