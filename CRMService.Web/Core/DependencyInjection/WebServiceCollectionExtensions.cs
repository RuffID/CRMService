namespace CRMService.Web.Core.DependencyInjection;

public static class WebServiceCollectionExtensions
{
    public static IServiceCollection AddWeb(
        this IServiceCollection services,
        WebApplicationBuilder builder)
    {
        services.AddWebMiddlewareServices();
        services.AddWebControllers();
        services.AddWebAuthentication(builder.Configuration);
        services.AddWebRazorPages();
        services.AddWebDataProtection(builder.Environment);
        services.AddWebSignalR();
        services.AddWebFilters();
        services.AddWebCoordinationServices();
        services.AddWebStartupInitializer();
        services.AddWebFileServices();
        services.AddProductionHostedServices();

        return services;
    }
}
