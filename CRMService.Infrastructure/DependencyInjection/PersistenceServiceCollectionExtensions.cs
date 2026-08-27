using CRMService.Application.Abstractions.Database.Repository;
using CRMService.Infrastructure.DataBase;
using CRMService.Infrastructure.DataBase.Repository;
using EFCoreLibrary.Abstractions.Database;
using EFCoreLibrary.EfCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CRMService.Infrastructure.DependencyInjection;

internal static class PersistenceServiceCollectionExtensions
{
    internal static IServiceCollection AddPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<MainContext>(options =>
        {
            options.UseSqlServer(configuration.GetConnectionString("MSSql"));
        });
        services.AddDbContext<OkdeskContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString("Postgresql"));
        });

        services.AddScoped<IAppDbContext<MainContext>>(serviceProvider =>
            new EfDbContextAdapter<MainContext>(serviceProvider.GetRequiredService<MainContext>()));
        services.AddScoped<IAppDbContext<OkdeskContext>>(serviceProvider =>
            new EfDbContextAdapter<OkdeskContext>(serviceProvider.GetRequiredService<OkdeskContext>()));
        services.AddScoped<IMainDbContextSession, MainDbContextSession>();
        services.AddScoped<IUnitOfWorkScope, MainDbUnitOfWorkScope>();
        services.AddScoped<IAuthorizationUnitOfWork, AuthorizationUnitOfWork>();
        services.AddScoped<IPlanSettingsUnitOfWork, PlanSettingsUnitOfWork>();
        services.AddScoped<IReportsUnitOfWork, ReportsUnitOfWork>();
        services.AddScoped<ICompanyDirectoryUnitOfWork, CompanyDirectoryUnitOfWork>();
        services.AddScoped<IEquipmentUnitOfWork, EquipmentUnitOfWork>();
        services.AddScoped<IIssuesUnitOfWork, IssuesUnitOfWork>();
        services.AddScoped<IOkdeskCompanyDirectorySource, OkdeskCompanyDirectorySource>();
        services.AddScoped<IOkdeskEquipmentSource, OkdeskEquipmentSource>();
        services.AddScoped<IOkdeskIssuesSource, OkdeskIssuesSource>();

        return services;
    }
}
