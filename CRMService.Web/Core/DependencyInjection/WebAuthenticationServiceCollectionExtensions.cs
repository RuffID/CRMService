using CRMService.Application.Models.ConfigClass;
using CRMService.Domain.Models.Constants;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using System.Text;

namespace CRMService.Web.Core.DependencyInjection;

internal static class WebAuthenticationServiceCollectionExtensions
{
    internal static IServiceCollection AddWebAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddAuthentication(options =>
        {
            options.DefaultScheme = "Smart";
            options.DefaultAuthenticateScheme = "Smart";
            options.DefaultChallengeScheme = "Smart";
        })
        .AddPolicyScheme("Smart", "Smart auth", options =>
        {
            options.ForwardDefaultSelector = context =>
            {
                string authorization = context.Request.Headers[HeaderNames.Authorization].ToString();
                if (!string.IsNullOrWhiteSpace(authorization)
                    && authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    return JwtBearerDefaults.AuthenticationScheme;
                }

                return CookieAuthenticationDefaults.AuthenticationScheme;
            };
        })
        .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
        {
            options.Cookie.Name = ".CRMService.Cookies";
            options.LoginPath = "/login";
            options.AccessDeniedPath = "/accessdenied";
            options.SlidingExpiration = true;
            options.ExpireTimeSpan = TimeSpan.FromDays(14);

            options.Events.OnRedirectToLogin = context =>
            {
                if (context.Request.Path.StartsWithSegments("/api"))
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return Task.CompletedTask;
                }

                context.Response.Redirect(context.RedirectUri);
                return Task.CompletedTask;
            };
            options.Events.OnRedirectToAccessDenied = context =>
            {
                if (context.Request.Path.StartsWithSegments("/api"))
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    return Task.CompletedTask;
                }

                context.Response.Redirect(context.RedirectUri);
                return Task.CompletedTask;
            };
        })
        .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
        {
            string key = configuration[AuthorizationOptions.SectionName]!;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = JWTSettingsConstants.ISSUER,
                ValidateAudience = true,
                ValidAudience = JWTSettingsConstants.AUDIENCE,
                ValidateLifetime = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                ValidateIssuerSigningKey = true,
            };
            options.RequireHttpsMetadata = false;
        });

        return services;
    }
}
