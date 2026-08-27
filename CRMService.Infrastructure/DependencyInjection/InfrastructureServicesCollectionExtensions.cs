using CRMService.Application.Abstractions.Service;
using CRMService.Infrastructure.DataBase;
using CRMService.Infrastructure.Service.Authorization;
using CRMService.Infrastructure.Service.DataBase;
using CRMService.Infrastructure.Service.Requests;
using HttpClientLibrary;
using HttpClientLibrary.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace CRMService.Infrastructure.DependencyInjection;

internal static class InfrastructureServicesCollectionExtensions
{
    internal static IServiceCollection AddDatabaseServices(this IServiceCollection services)
    {
        services.AddScoped<DataBaseCheckUpService<MainContext>>();
        services.AddScoped<SqlServerBackupService>();

        return services;
    }

    internal static IServiceCollection AddOkdeskHttpIntegration(this IServiceCollection services)
    {
        services.AddHttpClient<IHttpApiClient, HttpApiClient>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(180);
        });
        services.AddScoped<IOkdeskEntityRequestService, GetOkdeskEntityService>();

        return services;
    }

    internal static IServiceCollection AddAuthorizationInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IAccessTokenService, JwtTokenService>();

        return services;
    }

    internal static IServiceCollection AddTelegramIntegration(this IServiceCollection services)
    {
        services.AddHttpClient<INotificationService, TelegramNotification>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(180);
        });

        return services;
    }
}
