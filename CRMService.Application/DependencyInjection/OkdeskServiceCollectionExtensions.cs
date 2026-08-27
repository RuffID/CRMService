using CRMService.Application.Service.OkdeskEntity;
using CRMService.Application.Service.OkdeskEntity.Resolvers;
using Microsoft.Extensions.DependencyInjection;
using OkdeskRoleService = CRMService.Application.Service.OkdeskEntity.RoleService;

namespace CRMService.Application.DependencyInjection;

internal static class OkdeskServiceCollectionExtensions
{
    internal static IServiceCollection AddOkdeskServices(this IServiceCollection services)
    {
        services.AddScoped<CompanyCategoryService>();
        services.AddScoped<CompanyService>();
        services.AddScoped<EmployeeService>();
        services.AddScoped<EquipmentService>();
        services.AddScoped<GroupService>();
        services.AddScoped<ReferenceResolveHelper>();
        services.AddScoped<CompanyResolverService>();
        services.AddScoped<EmployeeResolverService>();
        services.AddScoped<IssuePriorityService>();
        services.AddScoped<IssueStatusResolverService>();
        services.AddScoped<IssueTypeResolverService>();
        services.AddScoped<IssuePriorityResolverService>();
        services.AddScoped<KindParameterResolverService>();
        services.AddScoped<KindResolverService>();
        services.AddScoped<ManufacturerResolverService>();
        services.AddScoped<ModelResolverService>();
        services.AddScoped<IssueService>();
        services.AddScoped<IssueStatusService>();
        services.AddScoped<IssueTypeService>();
        services.AddScoped<KindParameterService>();
        services.AddScoped<KindParamService>();
        services.AddScoped<KindService>();
        services.AddScoped<MaintenanceEntityService>();
        services.AddScoped<ManufacturerService>();
        services.AddScoped<ModelService>();
        services.AddScoped<MaintenanceEntityResolverService>();
        services.AddScoped<OkdeskRoleService>();
        services.AddScoped<TimeEntryService>();

        return services;
    }
}
