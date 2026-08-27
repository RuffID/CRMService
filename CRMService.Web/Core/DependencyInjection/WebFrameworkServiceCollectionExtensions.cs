using CRMService.Web.Core.Filter;
using CRMService.Web.Core.Middleware;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace CRMService.Web.Core.DependencyInjection;

internal static class WebFrameworkServiceCollectionExtensions
{
    internal static IServiceCollection AddWebMiddlewareServices(this IServiceCollection services)
    {
        services.AddTransient<ExceptionHandlingMiddleware>();

        return services;
    }

    internal static IServiceCollection AddWebControllers(this IServiceCollection services)
    {
        services.AddControllers();

        return services;
    }

    internal static IServiceCollection AddWebRazorPages(this IServiceCollection services)
    {
        services.AddRazorPages(options =>
        {
            // Делает все ссылки на страницы to lower case
            options.Conventions.AddFolderRouteModelConvention("/", model =>
            {
                foreach (SelectorModel selector in model.Selectors)
                {
                    AttributeRouteModel? attributeRoute = selector.AttributeRouteModel;
                    if (attributeRoute?.Template != null)
                        attributeRoute.Template = attributeRoute.Template.ToLowerInvariant();
                }
            });
        });

        return services;
    }

    internal static IServiceCollection AddWebSignalR(this IServiceCollection services)
    {
        services.AddSignalR();

        return services;
    }

    internal static IServiceCollection AddWebFilters(this IServiceCollection services)
    {
        services.AddScoped<IpOkdeskWebHookActionFilterAttribute>();

        return services;
    }
}
