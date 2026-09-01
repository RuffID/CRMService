using CRMService.Application.Models.ConfigClass;
using CRMService.Infrastructure.Service.DataBase;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CRMService.Infrastructure.DependencyInjection;

internal static class InfrastructureOptionsServiceCollectionExtensions
{
    internal static IServiceCollection AddInfrastructureOptions(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<ApiEndpointOptions>(
            configuration.GetSection(ApiEndpointOptions.SectionName));
        services.Configure<WebHookOkdeskOptions>(
            configuration.GetSection(WebHookOkdeskOptions.SectionName));
        services.Configure<OkdeskOptions>(options =>
        {
            options.OkdeskApiToken = configuration[OkdeskOptions.SectionName]!;
        });
        services.AddOptions<TelegramBotOptions>()
            .Bind(configuration.GetSection(TelegramBotOptions.SectionName))
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.Token),
                "TelegramBot:Token is missing in config.json")
            .ValidateOnStart();
        services.Configure<AuthorizationOptions>(options =>
        {
            options.JWTSymmetricSecurityKey = configuration[AuthorizationOptions.SectionName]!;
        });
        services.AddOptions<DatabaseBackupOptions>()
            .Bind(configuration.GetSection(DatabaseBackupOptions.SECTION_NAME))
            .Configure(options =>
                options.ConnectionString = configuration.GetConnectionString("MSSql") ?? string.Empty)
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.ConnectionString),
                "ConnectionStrings:MSSql is missing")
            .Validate(
                options => DatabaseBackupOptions.IsValidProjectName(options.ProjectName),
                "DatabaseBackup:ProjectName must contain only letters, digits, hyphens, or underscores")
            .Validate(
                options => DatabaseBackupOptions.IsAbsoluteWindowsSqlServerPath(options.WindowsSqlServerPath),
                "DatabaseBackup:WindowsSqlServerPath must be an absolute Windows SQL Server-visible path")
            .Validate(
                options => DatabaseBackupOptions.IsAbsoluteLinuxSqlServerPath(options.LinuxSqlServerPath),
                "DatabaseBackup:LinuxSqlServerPath must be an absolute Linux SQL Server-visible path")
            .Validate(
                _ => OperatingSystem.IsWindows() || OperatingSystem.IsLinux(),
                "Database backup is supported only on Windows and Linux")
            .ValidateOnStart();

        return services;
    }
}
