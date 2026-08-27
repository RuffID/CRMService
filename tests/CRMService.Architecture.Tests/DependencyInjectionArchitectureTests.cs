using System.Reflection;
using System.Runtime.CompilerServices;
using CRMService.Application.Abstractions.Database.Repository;
using CRMService.Application.Abstractions.Service;
using CRMService.Application.DependencyInjection;
using CRMService.Application.Models.ConfigClass;
using CRMService.Application.Service.Sync;
using CRMService.Application.Service.Webhook;
using CRMService.Infrastructure.DataBase;
using CRMService.Infrastructure.DataBase.Repository;
using CRMService.Infrastructure.DependencyInjection;
using CRMService.Infrastructure.Service.DataBase;
using CRMService.Web.Core.DependencyInjection;
using CRMService.Web.Core.Middleware;
using CRMService.Web.Core.Startup;
using CRMService.Web.Models.Server;
using CRMService.Web.Service.BackgroundServices;
using CRMService.Web.Service.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace CRMService.Architecture.Tests;

public class DependencyInjectionArchitectureTests
{
    [Fact]
    public void ProductionAssemblies_ExposeOnlyOneAggregateDiMethodPerLayer()
    {
        AssertAggregateMethod(
            typeof(ApplicationServiceCollectionExtensions),
            "AddApplication",
            typeof(IServiceCollection));
        AssertAggregateMethod(
            typeof(InfrastructureServiceCollectionExtensions),
            "AddInfrastructure",
            typeof(IServiceCollection),
            typeof(IConfiguration));
        AssertAggregateMethod(
            typeof(WebServiceCollectionExtensions),
            "AddWeb",
            typeof(IServiceCollection),
            typeof(WebApplicationBuilder));

        Assembly[] productionAssemblies = GetProductionAssemblies();
        Assert.DoesNotContain(
            productionAssemblies.SelectMany(assembly => assembly.GetTypes()).SelectMany(type => type.GetMethods()),
            method => method.IsPublic && method.IsStatic && method.Name == "ConfigureServices");
    }

    [Fact]
    public void AggregateRegistrations_DoNotRegisterImplementationsOwnedByAnotherProductionLayer()
    {
        Assembly applicationAssembly = typeof(ApplicationServiceCollectionExtensions).Assembly;
        Assembly infrastructureAssembly = typeof(InfrastructureServiceCollectionExtensions).Assembly;
        Assembly webAssembly = typeof(WebServiceCollectionExtensions).Assembly;

        ServiceCollection applicationServices = new();
        applicationServices.AddApplication();
        Assert.NotEmpty(applicationServices);
        Assert.All(
            applicationServices,
            descriptor => Assert.Equal(applicationAssembly, GetKnownImplementationType(descriptor)?.Assembly));

        ServiceCollection infrastructureServices = new();
        infrastructureServices.AddInfrastructure(CreateConfiguration());
        Assert.DoesNotContain(
            infrastructureServices.Select(GetKnownImplementationType),
            type => type?.Assembly == webAssembly);

        string contentRoot = Directory.CreateTempSubdirectory("crm-architecture-web-").FullName;
        try
        {
            WebApplicationBuilder builder = CreateTestingBuilder(contentRoot);
            ServiceCollection webServices = new();
            webServices.AddWeb(builder);

            Assert.DoesNotContain(
                webServices.Select(GetKnownImplementationType),
                type => type?.Assembly == applicationAssembly || type?.Assembly == infrastructureAssembly);
        }
        finally
        {
            Directory.Delete(contentRoot, recursive: true);
        }
    }

    [Fact]
    public void DomainAndContracts_DoNotContainDependencyInjectionExtensionClasses()
    {
        Assembly domainAssembly = typeof(CRMService.Domain.Models.Authorization.User).Assembly;
        Assembly contractsAssembly = typeof(CRMService.Contracts.Models.Responses.Results.ServiceResult).Assembly;

        Assert.DoesNotContain(domainAssembly.GetTypes(), IsDependencyInjectionExtensionClass);
        Assert.DoesNotContain(contractsAssembly.GetTypes(), IsDependencyInjectionExtensionClass);
    }

    [Fact]
    public void StartupInitializerContractAndImplementation_AreOwnedByWeb()
    {
        Assembly webAssembly = typeof(WebServiceCollectionExtensions).Assembly;

        Assert.Equal(webAssembly, typeof(IStartupInitializer).Assembly);
        Assert.Equal(webAssembly, typeof(MainDatabaseStartupInitializer).Assembly);
        Assert.DoesNotContain(
            typeof(InfrastructureServiceCollectionExtensions).Assembly.GetReferencedAssemblies(),
            reference => reference.Name == webAssembly.GetName().Name);
    }

    [Fact]
    public void ProductionAggregates_RegisterCriticalServicesOnceWithExpectedLifetimesAndOrder()
    {
        string contentRoot = Directory.CreateTempSubdirectory("crm-architecture-di-").FullName;
        try
        {
            WebApplicationBuilder builder = CreateTestingBuilder(contentRoot);
            IServiceCollection services = builder.Services;
            services.AddApplication();
            services.AddInfrastructure(builder.Configuration);
            services.AddWeb(builder);

            AssertSingleLifetime<MainContext>(services, ServiceLifetime.Scoped);
            AssertSingleLifetime<OkdeskContext>(services, ServiceLifetime.Scoped);
            AssertSingleLifetime(
                services,
                serviceType => IsAppDbContextFor(serviceType, typeof(MainContext)),
                ServiceLifetime.Scoped);
            AssertSingleLifetime(
                services,
                serviceType => IsAppDbContextFor(serviceType, typeof(OkdeskContext)),
                ServiceLifetime.Scoped);
            AssertSingleLifetime<IMainDbContextSession>(services, ServiceLifetime.Scoped);

            Type[] unitOfWorkTypes =
            [
                typeof(IUnitOfWorkScope),
                typeof(IAuthorizationUnitOfWork),
                typeof(IPlanSettingsUnitOfWork),
                typeof(IReportsUnitOfWork),
                typeof(ICompanyDirectoryUnitOfWork),
                typeof(IEquipmentUnitOfWork),
                typeof(IIssuesUnitOfWork)
            ];
            AssertScopedOnce(services, unitOfWorkTypes);

            Type[] sourceTypes =
            [
                typeof(IOkdeskCompanyDirectorySource),
                typeof(IOkdeskEquipmentSource),
                typeof(IOkdeskIssuesSource)
            ];
            AssertScopedOnce(services, sourceTypes);

            ServiceDescriptor[] repositories = services
                .Where(descriptor => descriptor.ServiceType.IsInterface
                    && descriptor.ServiceType.Assembly == typeof(IUnitOfWorkScope).Assembly
                    && descriptor.ServiceType.Namespace?.StartsWith(
                        "CRMService.Application.Abstractions.Database.Repository",
                        StringComparison.Ordinal) == true
                    && descriptor.ServiceType.Name.EndsWith("Repository", StringComparison.Ordinal))
                .ToArray();
            Assert.NotEmpty(repositories);
            Assert.All(repositories, descriptor => Assert.Equal(ServiceLifetime.Scoped, descriptor.Lifetime));
            Assert.Equal(repositories.Length, repositories.Select(descriptor => descriptor.ServiceType).Distinct().Count());

            AssertSingleLifetime<EntitySyncService>(services, ServiceLifetime.Singleton);
            AssertSingleLifetime<ServerData>(services, ServiceLifetime.Singleton);
            AssertSingleLifetime<BackgroundUpdateService>(services, ServiceLifetime.Singleton);
            AssertSingleLifetime<EquipmentCloudDbUpdateService>(services, ServiceLifetime.Singleton);
            AssertSingleLifetime<IReportBackgroundService>(services, ServiceLifetime.Singleton);
            AssertSingleLifetime<IStartupInitializer>(services, ServiceLifetime.Scoped);
            AssertSingleLifetime<ExceptionHandlingMiddleware>(services, ServiceLifetime.Transient);

            Assert.Single(
                services,
                descriptor => descriptor.ServiceType.FullName
                    == "HttpClientLibrary.Abstractions.IHttpApiClient");
            Assert.Single(services, descriptor => descriptor.ServiceType == typeof(INotificationService));
            AssertOptionsConfigurationCount<ApiEndpointOptions>(services, 1);
            AssertOptionsConfigurationCount<WebHookOkdeskOptions>(services, 1);
            AssertOptionsConfigurationCount<OkdeskOptions>(services, 1);
            AssertOptionsConfigurationCount<TelegramBotOptions>(services, 1);
            AssertOptionsConfigurationCount<AuthorizationOptions>(services, 1);
            AssertOptionsConfigurationCount<DatabaseBackupOptions>(services, 2);

            Type[] webhookHandlers = services
                .Where(descriptor => descriptor.ServiceType == typeof(IWebhookHandler))
                .Select(descriptor => descriptor.ImplementationType
                    ?? throw new InvalidOperationException("Production webhook handler must use ImplementationType."))
                .ToArray();
            Assert.Equal(
                [
                    typeof(IssueWebhookService),
                    typeof(CompanyWebhookService),
                    typeof(MaintenanceEntityWebhookService),
                    typeof(EquipmentWebhookService)
                ],
                webhookHandlers);
        }
        finally
        {
            Directory.Delete(contentRoot, recursive: true);
        }
    }

    [Fact]
    public void ProductionAggregates_ValidateScopesAndValidateOnBuild_InTestingWithoutPersistentKeys()
    {
        string contentRoot = Directory.CreateTempSubdirectory("crm-architecture-provider-").FullName;
        try
        {
            WebApplicationBuilder builder = CreateTestingBuilder(contentRoot);
            builder.Services.AddApplication();
            builder.Services.AddInfrastructure(builder.Configuration);
            builder.Services.AddWeb(builder);

            using ServiceProvider provider = builder.Services.BuildServiceProvider(new ServiceProviderOptions
            {
                ValidateScopes = true,
                ValidateOnBuild = true
            });

            Assert.NotNull(provider);
            Assert.False(Directory.Exists(Path.Combine(contentRoot, "keys-windows")));
            Assert.False(Directory.Exists(Path.Combine(contentRoot, "keys-linux")));
        }
        finally
        {
            Directory.Delete(contentRoot, recursive: true);
        }
    }

    private static void AssertAggregateMethod(Type extensionType, string methodName, params Type[] parameterTypes)
    {
        MethodInfo[] publicDiMethods = extensionType.Assembly
            .GetTypes()
            .SelectMany(type => type.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly))
            .Where(method => method.ReturnType == typeof(IServiceCollection)
                && method.GetParameters().FirstOrDefault()?.ParameterType == typeof(IServiceCollection))
            .ToArray();
        MethodInfo method = Assert.Single(publicDiMethods);

        Assert.Equal(extensionType, method.DeclaringType);
        Assert.Equal(methodName, method.Name);
        Assert.Equal(parameterTypes, method.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.True(method.IsDefined(typeof(ExtensionAttribute), inherit: false));
    }

    private static Assembly[] GetProductionAssemblies() =>
    [
        typeof(CRMService.Domain.Models.Authorization.User).Assembly,
        typeof(CRMService.Contracts.Models.Responses.Results.ServiceResult).Assembly,
        typeof(ApplicationServiceCollectionExtensions).Assembly,
        typeof(InfrastructureServiceCollectionExtensions).Assembly,
        typeof(WebServiceCollectionExtensions).Assembly
    ];

    private static Type? GetKnownImplementationType(ServiceDescriptor descriptor) =>
        descriptor.ImplementationType ?? descriptor.ImplementationInstance?.GetType();

    private static bool IsDependencyInjectionExtensionClass(Type type) =>
        type.Name.EndsWith("ServiceCollectionExtensions", StringComparison.Ordinal)
        || type.Namespace?.EndsWith(".DependencyInjection", StringComparison.Ordinal) == true;

    private static void AssertSingleLifetime<TService>(IServiceCollection services, ServiceLifetime lifetime)
    {
        ServiceDescriptor descriptor = Assert.Single(
            services,
            candidate => candidate.ServiceType == typeof(TService));
        Assert.Equal(lifetime, descriptor.Lifetime);
    }

    private static void AssertSingleLifetime(
        IServiceCollection services,
        Func<Type, bool> serviceTypePredicate,
        ServiceLifetime lifetime)
    {
        ServiceDescriptor descriptor = Assert.Single(
            services,
            candidate => serviceTypePredicate(candidate.ServiceType));
        Assert.Equal(lifetime, descriptor.Lifetime);
    }

    private static bool IsAppDbContextFor(Type serviceType, Type contextType) =>
        serviceType.IsGenericType
        && serviceType.GetGenericTypeDefinition().FullName
            == "EFCoreLibrary.Abstractions.Database.IAppDbContext`1"
        && serviceType.GetGenericArguments()[0] == contextType;

    private static void AssertScopedOnce(IServiceCollection services, IEnumerable<Type> serviceTypes)
    {
        foreach (Type serviceType in serviceTypes)
        {
            ServiceDescriptor descriptor = Assert.Single(
                services,
                candidate => candidate.ServiceType == serviceType);
            Assert.Equal(ServiceLifetime.Scoped, descriptor.Lifetime);
        }
    }

    private static void AssertOptionsConfigurationCount<TOptions>(
        IServiceCollection services,
        int expectedCount)
        where TOptions : class
    {
        Assert.Equal(
            expectedCount,
            services.Count(descriptor => descriptor.ServiceType == typeof(IConfigureOptions<TOptions>)));
    }

    private static WebApplicationBuilder CreateTestingBuilder(string contentRoot)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            EnvironmentName = "Testing",
            ContentRootPath = contentRoot
        });
        builder.Configuration.Sources.Clear();
        builder.Configuration.AddInMemoryCollection(CreateConfigurationValues());
        return builder;
    }

    private static IConfiguration CreateConfiguration()
    {
        ConfigurationManager configuration = new();
        configuration.AddInMemoryCollection(CreateConfigurationValues());
        return configuration;
    }

    private static Dictionary<string, string?> CreateConfigurationValues() => new()
    {
        ["ApiEndpoints:OkdeskDomainUrl"] = "https://okdesk.invalid",
        ["ApiEndpoints:TelegramBotUrl"] = "https://telegram.invalid/send",
        ["OkdeskApiToken"] = "test-okdesk-token",
        ["ConnectionStrings:MSSql"] = "Server=127.0.0.1,1;Database=architecture_tests;User Id=test;Password=test-only;Encrypt=False;Connect Timeout=1",
        ["ConnectionStrings:Postgresql"] = "Host=127.0.0.1;Port=1;Database=architecture_tests;Username=test;Password=test-only;Timeout=1",
        ["DatabaseBackup:ProjectName"] = "architecture_tests",
        ["DatabaseBackup:SqlServerPath"] = "/test-only/backups",
        ["JWTSymmetricSecurityKey"] = "test-only-signing-key-with-at-least-thirty-two-characters-1234567890",
        ["TelegramBot:SupportChatId"] = "1",
        ["TelegramBot:DebugChatId"] = "2",
        ["TelegramBot:Token"] = "test-telegram-token"
    };
}
