using CRMService.Application.Abstractions.Service;
using CRMService.Application.Service.Sync;
using CRMService.Application.Service.Webhook;
using Microsoft.Extensions.DependencyInjection;

namespace CRMService.Application.DependencyInjection;

internal static class SynchronizationAndWebhookServiceCollectionExtensions
{
    internal static IServiceCollection AddSynchronizationServices(this IServiceCollection services)
    {
        services.AddSingleton<EntitySyncService>();

        return services;
    }

    internal static IServiceCollection AddWebhookServices(this IServiceCollection services)
    {
        services.AddScoped<IWebhookHandler, IssueWebhookService>();
        services.AddScoped<IWebhookHandler, CompanyWebhookService>();
        services.AddScoped<IWebhookHandler, MaintenanceEntityWebhookService>();
        services.AddScoped<IWebhookHandler, EquipmentWebhookService>();

        return services;
    }
}
