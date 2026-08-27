using CRMService.Application.Abstractions.Database.Repository.Authorization;
using CRMService.Application.Abstractions.Database.Repository.CrmEntity;
using CRMService.Application.Abstractions.Database.Repository.Entity;
using CRMService.Application.Abstractions.Database.Repository.OkdeskEntity;
using CRMService.Application.Abstractions.Database.Repository.Report;
using CRMService.Infrastructure.DataBase;
using CRMService.Infrastructure.DataBase.Repository.Authorization;
using CRMService.Infrastructure.DataBase.Repository.CrmEntity;
using CRMService.Infrastructure.DataBase.Repository.Entity;
using CRMService.Infrastructure.DataBase.Repository.OkdeskEntity;
using CRMService.Infrastructure.DataBase.Repository.Report;
using EFCoreLibrary.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace CRMService.Infrastructure.DependencyInjection;

internal static class RepositoryServiceCollectionExtensions
{
    internal static IServiceCollection AddPersistenceRepositories(this IServiceCollection services)
    {
        services.AddEfCoreBaseRepositories<MainContext>();
        services.AddEfCoreBaseRepositories<OkdeskContext>();

        services.AddAuthorizationRepositories();
        services.AddCompanyDirectoryRepositories();
        services.AddEquipmentRepositories();
        services.AddIssueRepositories();
        services.AddOkdeskReadOnlyRepositories();
        services.AddReportRepositories();

        return services;
    }

    private static void AddAuthorizationRepositories(this IServiceCollection services)
    {
        services.AddScoped<IBlockReasonRepository, BlockReasonRepository>();
        services.AddScoped<ICrmRoleRepository, CrmRoleRepository>();
        services.AddScoped<ISessionRepository, SessionRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserRoleRepository, UserRoleRepository>();
    }

    private static void AddCompanyDirectoryRepositories(this IServiceCollection services)
    {
        services.AddScoped<ICompanyRepository, CompanyRepository>();
        services.AddScoped<ICompanyCategoryRepository, CategoryRepository>();
        services.AddScoped<IEmployeeGroupRepository, EmployeeGroupRepository>();
        services.AddScoped<IEmployeeRepository, EmployeeRepository>();
        services.AddScoped<IEmployeeRoleRepository, EmployeeRoleRepository>();
        services.AddScoped<IGroupRepository, GroupRepository>();
    }

    private static void AddEquipmentRepositories(this IServiceCollection services)
    {
        services.AddScoped<IEquipmentRepository, EquipmentRepository>();
        services.AddScoped<IKindParameterRepository, KindParameterRepository>();
        services.AddScoped<IKindRepository, KindRepository>();
        services.AddScoped<IKindParamsRepository, KindParamsRepository>();
        services.AddScoped<IMaintenanceEntityRepository, MaintenanceEntityRepository>();
        services.AddScoped<IManufacturerRepository, ManufacturerRepository>();
        services.AddScoped<IModelRepository, ModelRepository>();
        services.AddScoped<IParameterRepository, ParameterRepository>();
    }

    private static void AddIssueRepositories(this IServiceCollection services)
    {
        services.AddScoped<IIssuePriorityRepository, IssuePriorityRepository>();
        services.AddScoped<IIssueTypeRepository, IssueTypeRepository>();
        services.AddScoped<IIssueTypeGroupRepository, IssueTypeGroupRepository>();
        services.AddScoped<IIssueStatusRepository, IssueStatusRepository>();
        services.AddScoped<IIssueRepository, IssueRepository>();
        services.AddScoped<IOkdeskRoleRepository, OkdeskRoleRepository>();
        services.AddScoped<ITimeEntryRepository, TimeEntryRepository>();
    }

    private static void AddOkdeskReadOnlyRepositories(this IServiceCollection services)
    {
        services.AddScoped<IOkdeskCompanyCategoryRepository, OkdeskCompanyCategoryRepository>();
        services.AddScoped<IOkdeskCompanyRepository, OkdeskCompanyRepository>();
        services.AddScoped<IOkdeskEmployeeRepository, OkdeskEmployeeRepository>();
        services.AddScoped<IOkdeskGroupRepository, OkdeskGroupRepository>();
        services.AddScoped<IOkdeskKindRepository, OkdeskKindRepository>();
        services.AddScoped<IOkdeskKindParameterRepository, OkdeskKindParameterRepository>();
        services.AddScoped<IOkdeskKindParamsRepository, OkdeskKindParamsRepository>();
        services.AddScoped<IOkdeskMaintenanceEntityRepository, OkdeskMaintenanceEntityRepository>();
        services.AddScoped<IOkdeskManufacturerRepository, OkdeskManufacturerRepository>();
        services.AddScoped<IOkdeskModelRepository, OkdeskModelRepository>();
        services.AddScoped<IOkdeskEquipmentRepository, OkdeskEquipmentRepository>();
        services.AddScoped<IOkdeskIssueRepository, OkdeskIssueRepository>();
        services.AddScoped<IOkdeskIssueStatusRepository, OkdeskIssueStatusRepository>();
        services.AddScoped<IOkdeskIssuePriorityRepository, OkdeskIssuePriorityRepository>();
        services.AddScoped<IOkdeskIssueTypeRepository, OkdeskIssueTypeRepository>();
        services.AddScoped<IOkdeskIssueTypeGroupRepository, OkdeskIssueTypeGroupRepository>();
        services.AddScoped<IOkdeskTimeEntryRepository, OkdeskTimeEntryRepository>();
    }

    private static void AddReportRepositories(this IServiceCollection services)
    {
        services.AddScoped<IEmployeePerformanceReportRepository, EmployeePerformanceReportRepository>();
        services.AddScoped<ISpentTimeChartReportRepository, SpentTimeChartReportRepository>();
        services.AddScoped<IIssueDynamicsChartReportRepository, IssueDynamicsChartReportRepository>();
        services.AddScoped<IPlanRepository, PlanRepository>();
        services.AddScoped<IGeneralSettingsRepository, GeneralSettingsRepository>();
        services.AddScoped<IPlanSettingRepository, PlanSettingRepository>();
        services.AddScoped<IPlanColorSchemeRepository, PlanColorSchemeRepository>();
    }
}
