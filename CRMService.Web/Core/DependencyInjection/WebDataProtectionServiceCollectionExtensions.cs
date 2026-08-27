using Microsoft.AspNetCore.DataProtection;
using System.Reflection;
using System.Runtime.InteropServices;

namespace CRMService.Web.Core.DependencyInjection;

internal static class WebDataProtectionServiceCollectionExtensions
{
    internal static IServiceCollection AddWebDataProtection(
        this IServiceCollection services,
        IWebHostEnvironment environment)
    {
        string projectName = Assembly.GetEntryAssembly()?.GetName().Name ?? "DefaultAppName";

        if (environment.IsEnvironment("Testing"))
        {
            services.AddDataProtection()
                .UseEphemeralDataProtectionProvider()
                .SetApplicationName(projectName);
            return services;
        }

        string keyPath = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? Path.Combine(environment.ContentRootPath, "keys-windows")
            : Path.Combine(environment.ContentRootPath, "keys-linux");

        Directory.CreateDirectory(keyPath);

        services.AddDataProtection()
            .PersistKeysToFileSystem(new DirectoryInfo(keyPath))
            .SetApplicationName(projectName);

        return services;
    }
}
