using CRMService.Application.Abstractions.Service;
using CRMService.Web.Core.Startup;
using CRMService.Web.Models.Server;
using CRMService.Web.Service.BackgroundServices;
using CRMService.Web.Service.Settings;

namespace CRMService.Web.Core.DependencyInjection;

internal static class WebOwnedServiceCollectionExtensions
{
    internal static IServiceCollection AddWebCoordinationServices(this IServiceCollection services)
    {
        services.AddSingleton<ServerData>();
        services.AddSingleton<BackgroundUpdateService>();
        services.AddSingleton<EquipmentCloudDbUpdateService>();

        return services;
    }

    internal static IServiceCollection AddWebStartupInitializer(this IServiceCollection services)
    {
        services.AddScoped<IStartupInitializer, MainDatabaseStartupInitializer>();

        return services;
    }

    internal static IServiceCollection AddWebFileServices(this IServiceCollection services)
    {
        services.AddSingleton<IReportBackgroundService, ReportBackgroundService>();

        return services;
    }

    internal static IServiceCollection AddProductionHostedServices(this IServiceCollection services)
    {
#if !DEBUG
        services.AddHostedService<ThirtyMinutesReportHostedService>();
        services.AddHostedService<DailyReportHostedService>();
        services.AddHostedService<UpdateDirectoriesHostedService>();
#endif

        return services;
    }
}
