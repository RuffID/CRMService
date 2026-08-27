using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CRMService.Infrastructure.DependencyInjection;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddInfrastructureOptions(configuration);
        services.AddPersistenceRepositories();
        services.AddPersistence(configuration);
        services.AddDatabaseServices();
        services.AddOkdeskHttpIntegration();
        services.AddAuthorizationInfrastructure();
        services.AddTelegramIntegration();

        return services;
    }
}
