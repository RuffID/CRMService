using CRMService.Application.Abstractions.Service;
using CRMService.Application.Service.Authorization;
using CRMService.Application.Service.CrmServices;
using CRMService.Application.Service.OkdeskEntity;
using CRMService.Infrastructure.DataBase;
using CRMService.Web.Core.Startup;
using CRMService.Web.IntegrationTests.Infrastructure;
using CRMService.Web.Service.BackgroundServices;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;

namespace CRMService.Web.IntegrationTests.Hosting;

public class DependencyInjectionTests
{
    [Fact]
    public void TestHost_ValidatesAndResolvesKeyServicesWithTestReplacements()
    {
        using CrmWebApplicationFactory factory = new();
        IServiceProvider root = factory.Services;
        using IServiceScope scope = root.CreateScope();
        IServiceProvider services = scope.ServiceProvider;

        MainContext mainContext = services.GetRequiredService<MainContext>();
        OkdeskContext okdeskContext = services.GetRequiredService<OkdeskContext>();

        Assert.Equal("Microsoft.EntityFrameworkCore.SqlServer", mainContext.Database.ProviderName);
        Assert.Equal("Npgsql.EntityFrameworkCore.PostgreSQL", okdeskContext.Database.ProviderName);
        Assert.Contains("crm_web_tests", mainContext.Database.GetConnectionString(), StringComparison.Ordinal);
        Assert.Contains("crm_web_tests", okdeskContext.Database.GetConnectionString(), StringComparison.Ordinal);
        Assert.IsType<RecordingStartupInitializer>(services.GetRequiredService<IStartupInitializer>());
        Assert.Same(factory.HttpApiClient, services.GetRequiredService(factory.HttpApiClientServiceType));
        Assert.Same(factory.NotificationService, services.GetRequiredService<INotificationService>());
        Assert.NotNull(services.GetRequiredService<AuthenticationService>());
        Assert.NotNull(services.GetRequiredService<IPlanSettingsService>());
        Assert.NotNull(services.GetRequiredService<IssueService>());
        Assert.NotNull(services.GetRequiredService<IReportBackgroundService>());
        Assert.Equal("EphemeralDataProtectionProvider", services.GetRequiredService<IDataProtectionProvider>().GetType().Name);
        Assert.Equal("test-okdesk-token", services.GetRequiredService<IConfiguration>()["OkdeskApiToken"]);
        Assert.Equal(1, factory.StartupInitializer.InitializeCount);

        Type[] hostedTypes = root.GetServices<IHostedService>().Select(service => service.GetType()).ToArray();
        Assert.DoesNotContain(typeof(ThirtyMinutesReportHostedService), hostedTypes);
        Assert.DoesNotContain(typeof(DailyReportHostedService), hostedTypes);
        Assert.DoesNotContain(typeof(UpdateDirectoriesHostedService), hostedTypes);
    }
}
