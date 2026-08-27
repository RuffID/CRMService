using CRMService.Application.Abstractions.Service;
using CRMService.Application.Service.Hosted;
using Microsoft.Extensions.DependencyInjection;

namespace CRMService.Application.DependencyInjection;

internal static class DirectoryUpdateServiceCollectionExtensions
{
    internal static IServiceCollection AddDirectoryUpdateServices(this IServiceCollection services)
    {
        services.AddScoped<IDirectoryUpdateOperations, DirectoryUpdateOperations>();
        services.AddScoped<UpdateDirectoriesService>();

        return services;
    }
}
