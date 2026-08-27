using Microsoft.Extensions.DependencyInjection;

namespace CRMService.Application.DependencyInjection;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSynchronizationServices();
        services.AddOkdeskServices();
        services.AddReportServices();
        services.AddPlanSettingsServices();
        services.AddDirectoryUpdateServices();
        services.AddAuthorizationServices();
        services.AddWebhookServices();

        return services;
    }
}
