using CRMService.Application.Abstractions.Database.Repository;
using CRMService.Application.Abstractions.Database.Repository.Authorization;
using CRMService.Application.Abstractions.Database.Repository.Entity;
using CRMService.Application.Abstractions.Database.Repository.Report;
using CRMService.Infrastructure.DataBase.Repository;
using CRMService.Infrastructure.DataBase.Repository.Authorization;
using CRMService.Infrastructure.DataBase.Repository.Entity;
using CRMService.Infrastructure.DataBase.Repository.Report;
using CRMService.Infrastructure.DataBase;
using EFCoreLibrary.Abstractions.Database;
using EFCoreLibrary.EfCore;
using EFCoreLibrary.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CRMService.Infrastructure.IntegrationTests.Database;

internal static class MainTestServices
{
    public static ServiceProvider Create(string connectionString)
    {
        ServiceCollection services = new();
        services.AddDbContext<MainContext>(options => options.UseSqlServer(connectionString));
        services.AddScoped<IAppDbContext<MainContext>>(provider =>
            new EfDbContextAdapter<MainContext>(provider.GetRequiredService<MainContext>()));
        services.AddEfCoreBaseRepositories<MainContext>();

        services.AddScoped<IMainDbContextSession, MainDbContextSession>();
        services.AddScoped<IUnitOfWorkScope, MainDbUnitOfWorkScope>();
        services.AddScoped<IAuthorizationUnitOfWork, AuthorizationUnitOfWork>();

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ICrmRoleRepository, CrmRoleRepository>();
        services.AddScoped<ISessionRepository, SessionRepository>();
        services.AddScoped<IEmployeeRepository, EmployeeRepository>();
        services.AddScoped<IIssueRepository, IssueRepository>();
        services.AddScoped<IEquipmentRepository, EquipmentRepository>();
        services.AddScoped<IEmployeePerformanceReportRepository, EmployeePerformanceReportRepository>();
        services.AddScoped<ISpentTimeChartReportRepository, SpentTimeChartReportRepository>();
        services.AddScoped<IIssueDynamicsChartReportRepository, IssueDynamicsChartReportRepository>();

        return services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateOnBuild = true,
            ValidateScopes = true
        });
    }
}
