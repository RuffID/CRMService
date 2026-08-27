using CRMService.Application.Abstractions.Service;
using CRMService.Application.Service.Authorization;
using Microsoft.Extensions.DependencyInjection;
using AuthorizationRoleService = CRMService.Application.Service.Authorization.RoleService;

namespace CRMService.Application.DependencyInjection;

internal static class AuthorizationServiceCollectionExtensions
{
    internal static IServiceCollection AddAuthorizationServices(this IServiceCollection services)
    {
        services.AddScoped<Hasher>();
        services.AddScoped<IRandomStringGenerator, GenerateRandomString>();
        services.AddScoped<UserService>();
        services.AddScoped<AuthorizationRoleService>();
        services.AddScoped<AuthenticationService>();

        return services;
    }
}
