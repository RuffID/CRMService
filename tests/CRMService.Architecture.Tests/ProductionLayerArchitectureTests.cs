using System.Reflection;
using CRMService.Application.Abstractions.Database.Repository;
using CRMService.Application.DependencyInjection;
using CRMService.Infrastructure.DependencyInjection;
using CRMService.Web.Core.DependencyInjection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CRMService.Architecture.Tests;

public class ProductionLayerArchitectureTests
{
    private static readonly IReadOnlyDictionary<Type, HashSet<string>> SCENARIO_REPOSITORIES =
        new Dictionary<Type, HashSet<string>>
        {
            [typeof(IAuthorizationUnitOfWork)] =
            [
                "IUserRepository",
                "ICrmRoleRepository",
                "ISessionRepository",
                "IEmployeeRepository"
            ],
            [typeof(IPlanSettingsUnitOfWork)] =
            [
                "IPlanRepository",
                "IGeneralSettingsRepository",
                "IPlanSettingRepository",
                "IPlanColorSchemeRepository"
            ],
            [typeof(IReportsUnitOfWork)] =
            [
                "IEmployeePerformanceReportRepository",
                "ISpentTimeChartReportRepository",
                "IIssueDynamicsChartReportRepository",
                "IEmployeeRepository",
                "IEmployeeGroupRepository",
                "IGroupRepository",
                "IPlanRepository",
                "IPlanSettingRepository"
            ],
            [typeof(ICompanyDirectoryUnitOfWork)] =
            [
                "ICompanyRepository",
                "ICompanyCategoryRepository",
                "IEmployeeRepository",
                "IEmployeeGroupRepository",
                "IEmployeeRoleRepository",
                "IGroupRepository",
                "IOkdeskRoleRepository"
            ],
            [typeof(IEquipmentUnitOfWork)] =
            [
                "IEquipmentRepository",
                "IParameterRepository",
                "IKindRepository",
                "IKindParamsRepository",
                "IKindParameterRepository",
                "IMaintenanceEntityRepository",
                "IManufacturerRepository",
                "IModelRepository",
                "ICompanyRepository"
            ],
            [typeof(IIssuesUnitOfWork)] =
            [
                "IIssueRepository",
                "IIssuePriorityRepository",
                "IIssueStatusRepository",
                "IIssueTypeRepository",
                "IIssueTypeGroupRepository",
                "ITimeEntryRepository",
                "IEmployeeRepository",
                "ICompanyRepository",
                "IMaintenanceEntityRepository"
            ]
        };

    [Fact]
    public void MvcControllersAndPageModels_AreOwnedOnlyByWeb()
    {
        Assembly webAssembly = typeof(WebServiceCollectionExtensions).Assembly;
        Type[] mvcTypes = GetProductionAssemblies()
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => !type.IsAbstract
                && (typeof(ControllerBase).IsAssignableFrom(type)
                    || typeof(PageModel).IsAssignableFrom(type)))
            .ToArray();

        Assert.NotEmpty(mvcTypes);
        Assert.All(mvcTypes, type => Assert.Equal(webAssembly, type.Assembly));
    }

    [Fact]
    public void RepositoryImplementationsAndDbContexts_AreOwnedOnlyByInfrastructure()
    {
        Assembly applicationAssembly = typeof(ApplicationServiceCollectionExtensions).Assembly;
        Assembly infrastructureAssembly = typeof(InfrastructureServiceCollectionExtensions).Assembly;
        Type[] repositoryContracts = applicationAssembly
            .GetTypes()
            .Where(type => type.IsInterface
                && type.Name.EndsWith("Repository", StringComparison.Ordinal)
                && type.Namespace?.StartsWith(
                    "CRMService.Application.Abstractions.Database.Repository",
                    StringComparison.Ordinal) == true)
            .ToArray();
        Type[] productionTypes = GetProductionAssemblies()
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => type.IsClass && !type.IsAbstract)
            .ToArray();
        Type[] repositoryImplementations = productionTypes
            .Where(type => repositoryContracts.Any(contract => contract.IsAssignableFrom(type)))
            .ToArray();
        Type[] dbContexts = productionTypes
            .Where(type => typeof(DbContext).IsAssignableFrom(type))
            .ToArray();

        Assert.NotEmpty(repositoryImplementations);
        Assert.All(repositoryImplementations, type => Assert.Equal(infrastructureAssembly, type.Assembly));
        Assert.NotEmpty(dbContexts);
        Assert.All(dbContexts, type => Assert.Equal(infrastructureAssembly, type.Assembly));
    }

    [Fact]
    public void ApplicationServices_Constructors_DoNotDependOnInfrastructureImplementations()
    {
        Assembly applicationAssembly = typeof(ApplicationServiceCollectionExtensions).Assembly;
        Assembly infrastructureAssembly = typeof(InfrastructureServiceCollectionExtensions).Assembly;
        Type[] forbiddenParameters = applicationAssembly
            .GetTypes()
            .Where(type => type.IsClass
                && !type.IsAbstract
                && type.Namespace?.StartsWith("CRMService.Application.Service", StringComparison.Ordinal) == true)
            .SelectMany(type => type.GetConstructors())
            .SelectMany(constructor => constructor.GetParameters())
            .SelectMany(parameter => FlattenType(parameter.ParameterType))
            .Where(type => type.Assembly == infrastructureAssembly)
            .Distinct()
            .ToArray();

        Assert.Empty(forbiddenParameters);
    }

    [Fact]
    public void LowerLayers_DoNotReferenceWebAssembly()
    {
        string webAssemblyName = typeof(WebServiceCollectionExtensions).Assembly.GetName().Name!;
        Assembly[] lowerLayers =
        [
            typeof(CRMService.Domain.Models.Authorization.User).Assembly,
            typeof(CRMService.Contracts.Models.Responses.Results.ServiceResult).Assembly,
            typeof(ApplicationServiceCollectionExtensions).Assembly,
            typeof(InfrastructureServiceCollectionExtensions).Assembly
        ];

        Assert.All(
            lowerLayers,
            assembly => Assert.DoesNotContain(
                assembly.GetReferencedAssemblies(),
                reference => reference.Name == webAssemblyName));
    }

    [Fact]
    public void DomainAndContracts_DoNotReferenceDatabaseOrHttpImplementations()
    {
        Assembly[] coreAssemblies =
        [
            typeof(CRMService.Domain.Models.Authorization.User).Assembly,
            typeof(CRMService.Contracts.Models.Responses.Results.ServiceResult).Assembly
        ];
        string[] forbiddenReferences =
        [
            "HttpClientLibrary",
            "Microsoft.EntityFrameworkCore",
            "Microsoft.EntityFrameworkCore.Relational",
            "Microsoft.EntityFrameworkCore.SqlServer",
            "Npgsql",
            "Npgsql.EntityFrameworkCore.PostgreSQL"
        ];

        Assert.All(
            coreAssemblies,
            assembly => Assert.DoesNotContain(
                assembly.GetReferencedAssemblies(),
                reference => forbiddenReferences.Contains(reference.Name, StringComparer.Ordinal)));
    }

    [Fact]
    public void ScenarioUnitOfWorkContracts_ExposeOnlyTheirApprovedRepositories()
    {
        foreach ((Type unitOfWorkType, HashSet<string> expectedRepositories) in SCENARIO_REPOSITORIES)
        {
            HashSet<string> actualRepositories = unitOfWorkType
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Select(property => property.PropertyType.Name)
                .ToHashSet(StringComparer.Ordinal);

            Assert.True(
                expectedRepositories.SetEquals(actualRepositories),
                $"{unitOfWorkType.Name} repositories differ. "
                + $"Expected: {string.Join(", ", expectedRepositories.Order())}. "
                + $"Actual: {string.Join(", ", actualRepositories.Order())}.");
        }
    }

    private static Assembly[] GetProductionAssemblies() =>
    [
        typeof(CRMService.Domain.Models.Authorization.User).Assembly,
        typeof(CRMService.Contracts.Models.Responses.Results.ServiceResult).Assembly,
        typeof(ApplicationServiceCollectionExtensions).Assembly,
        typeof(InfrastructureServiceCollectionExtensions).Assembly,
        typeof(WebServiceCollectionExtensions).Assembly
    ];

    private static IEnumerable<Type> FlattenType(Type type)
    {
        yield return type;

        if (!type.IsGenericType)
            yield break;

        foreach (Type argument in type.GetGenericArguments().SelectMany(FlattenType))
            yield return argument;
    }
}
