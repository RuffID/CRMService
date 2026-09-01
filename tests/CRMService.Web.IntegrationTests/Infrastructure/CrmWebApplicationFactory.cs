using CRMService.Application.Abstractions.Database.Repository;
using CRMService.Application.Abstractions.Database.Repository.Authorization;
using CRMService.Application.Abstractions.Service;
using CRMService.Application.Models.WebHook;
using CRMService.Domain.Models.Authorization;
using CRMService.Infrastructure.DataBase;
using CRMService.Web.Core.Startup;
using CRMService.Web.IntegrationTests.TestEndpoints;
using CRMService.Web.Service.BackgroundServices;
using CRMService.Web.Service.Settings;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSubstitute;

namespace CRMService.Web.IntegrationTests.Infrastructure;

public class CrmWebApplicationFactory : WebApplicationFactory<ReportBackgroundService>
{
    public static readonly Guid TEST_USER_ID = Guid.Parse("7caa166e-cc22-4870-a960-e8bfb5be0c75");

    public RecordingStartupInitializer StartupInitializer { get; } = new();

    public object HttpApiClient { get; private set; } = null!;

    public Type HttpApiClientServiceType { get; private set; } = null!;

    public NoNetworkDispatchProxy HttpApiClientControl { get; private set; } = null!;

    public ControlledNotificationService NotificationService { get; } = new();

    public ControlledWebhookHandler FirstWebhookHandler { get; } = new(_ => false);

    public ControlledWebhookHandler MatchingWebhookHandler { get; } = new(
        @event => string.Equals(@event.Event?.Event_type, "issue_created", StringComparison.Ordinal));

    public ControlledWebhookHandler LastWebhookHandler { get; } = new(_ => false);

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseDefaultServiceProvider(options =>
        {
            options.ValidateScopes = true;
            options.ValidateOnBuild = true;
        });

        builder.ConfigureAppConfiguration(configuration =>
        {
            configuration.Sources.Clear();
            configuration.AddInMemoryCollection(CreateConfiguration());
        });

        builder.ConfigureServices(services =>
        {
            services.AddSingleton<IHostedService, ThirtyMinutesReportHostedService>();
            services.AddSingleton<IHostedService, DailyReportHostedService>();
            services.AddSingleton<IHostedService, UpdateDirectoriesHostedService>();
        });

        builder.ConfigureTestServices(services =>
        {
            ReplaceDatabaseContexts(services);
            RemoveProductionHostedServices(services);
            ReplaceStartupInitializer(services);
            ReplaceExternalServices(services);
            ConfigureTestAuthentication(services);
            ConfigureCurrentUser(services);
            ConfigureWebhookHandlers(services);

            services.AddSingleton<IStartupFilter, TestProxyStartupFilter>();
            services.AddControllers().AddApplicationPart(typeof(TestProbeController).Assembly);
        });
    }

    private static Dictionary<string, string?> CreateConfiguration() => new()
    {
        ["ApiEndpoints:OkdeskDomainUrl"] = "https://okdesk.invalid",
        ["ApiEndpoints:TelegramBotUrl"] = "https://telegram.invalid/send",
        ["OkdeskApiToken"] = "test-okdesk-token",
        ["ConnectionStrings:MSSql"] = "Server=127.0.0.1,1;Database=crm_web_tests;User Id=test;Password=test-only;Encrypt=False;Connect Timeout=1",
        ["ConnectionStrings:Postgresql"] = "Host=127.0.0.1;Port=1;Database=crm_web_tests;Username=test;Password=test-only;Timeout=1",
        ["DatabaseBackup:ProjectName"] = "crm_web_tests",
        ["DatabaseBackup:WindowsSqlServerPath"] = "C:\\test-only\\backups",
        ["DatabaseBackup:LinuxSqlServerPath"] = "/test-only/backups",
        ["JWTSymmetricSecurityKey"] = "test-only-signing-key-with-at-least-thirty-two-characters-1234567890",
        ["TelegramBot:SupportChatId"] = "1",
        ["TelegramBot:DebugChatId"] = "2",
        ["TelegramBot:Token"] = "test-telegram-token",
        ["WebHookOkdeskIpAddressList:IpAddressList:0"] = "203.0.113.10",
        ["WebHookOkdeskIpAddressList:IpAddressList:1"] = "198.51.100.0/24"
    };

    private static void ReplaceDatabaseContexts(IServiceCollection services)
    {
        services.RemoveAll<MainContext>();
        services.RemoveAll<DbContextOptions<MainContext>>();
        services.RemoveAll<IDbContextOptionsConfiguration<MainContext>>();
        services.RemoveAll<OkdeskContext>();
        services.RemoveAll<DbContextOptions<OkdeskContext>>();
        services.RemoveAll<IDbContextOptionsConfiguration<OkdeskContext>>();

        services.AddDbContext<MainContext>(options => options.UseSqlServer(
            "Server=127.0.0.1,1;Database=crm_web_tests;User Id=test;Password=test-only;Encrypt=False;Connect Timeout=1"));
        services.AddDbContext<OkdeskContext>(options => options.UseSqlServer(
            "Server=127.0.0.1,1;Database=crm_web_tests;User Id=test;Password=test-only;Encrypt=False;Connect Timeout=1"));

    }

    private static void RemoveProductionHostedServices(IServiceCollection services)
    {
        Type[] productionHostedServiceTypes =
        [
            typeof(ThirtyMinutesReportHostedService),
            typeof(DailyReportHostedService),
            typeof(UpdateDirectoriesHostedService)
        ];

        ServiceDescriptor[] descriptors = services
            .Where(descriptor =>
                descriptor.ServiceType == typeof(IHostedService)
                && descriptor.ImplementationType is not null
                && productionHostedServiceTypes.Contains(descriptor.ImplementationType))
            .ToArray();

        foreach (ServiceDescriptor descriptor in descriptors)
            services.Remove(descriptor);
    }

    private void ReplaceStartupInitializer(IServiceCollection services)
    {
        services.RemoveAll<IStartupInitializer>();
        services.AddSingleton<IStartupInitializer>(StartupInitializer);
    }

    private void ReplaceExternalServices(IServiceCollection services)
    {
        services.RemoveAll<INotificationService>();
        services.AddSingleton<INotificationService>(NotificationService);

        ServiceDescriptor httpApiDescriptor = services.Single(descriptor =>
            descriptor.ServiceType.FullName == "HttpClientLibrary.Abstractions.IHttpApiClient");
        Type httpApiClientType = httpApiDescriptor.ServiceType;
        HttpApiClientServiceType = httpApiClientType;
        services.Remove(httpApiDescriptor);

        HttpApiClient = System.Reflection.DispatchProxy.Create(httpApiClientType, typeof(NoNetworkDispatchProxy));
        HttpApiClientControl = (NoNetworkDispatchProxy)HttpApiClient;
        services.Add(ServiceDescriptor.Singleton(httpApiClientType, HttpApiClient));
    }

    private static void ConfigureTestAuthentication(IServiceCollection services)
    {
        services.AddAuthentication()
            .AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>(
                TestAuthenticationHandler.SCHEME_NAME,
                _ => { });

        services.PostConfigure<PolicySchemeOptions>("Smart", options =>
        {
            Func<HttpContext, string?>? productionSelector = options.ForwardDefaultSelector;
            options.ForwardDefaultSelector = context =>
            {
                if (context.Request.Headers.ContainsKey(TestAuthenticationHandler.USER_HEADER))
                    return TestAuthenticationHandler.SCHEME_NAME;

                return productionSelector?.Invoke(context)
                    ?? CookieAuthenticationDefaults.AuthenticationScheme;
            };
        });
    }

    private static void ConfigureCurrentUser(IServiceCollection services)
    {
        IUserRepository userRepository = Substitute.For<IUserRepository>();
        User user = new()
        {
            Id = TEST_USER_ID,
            Name = "Integration Test User",
            Login = "integration-test",
            Active = true,
            Roles = new List<CrmRole>
            {
                new() { Id = Guid.Parse("46d3c4aa-d542-4245-bced-26cf4b92193e"), Name = "admin" }
            }
        };

        userRepository
            .GetByIdWithRolesReadOnlyAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(user);

        IAuthorizationUnitOfWork unitOfWork = Substitute.For<IAuthorizationUnitOfWork>();
        unitOfWork.User.Returns(userRepository);

        services.RemoveAll<IAuthorizationUnitOfWork>();
        services.AddSingleton(unitOfWork);
    }

    private void ConfigureWebhookHandlers(IServiceCollection services)
    {
        services.RemoveAll<IWebhookHandler>();
        services.AddSingleton<IWebhookHandler>(FirstWebhookHandler);
        services.AddSingleton<IWebhookHandler>(MatchingWebhookHandler);
        services.AddSingleton<IWebhookHandler>(LastWebhookHandler);
    }
}
