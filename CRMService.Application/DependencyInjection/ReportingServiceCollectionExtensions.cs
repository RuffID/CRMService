using CRMService.Application.Abstractions.Service;
using CRMService.Application.Service.CrmServices;
using CRMService.Application.Service.Report;
using Microsoft.Extensions.DependencyInjection;

namespace CRMService.Application.DependencyInjection;

internal static class ReportingServiceCollectionExtensions
{
    internal static IServiceCollection AddReportServices(this IServiceCollection services)
    {
        services.AddScoped<IEmployeePerformanceReportService, EmployeePerformanceReportService>();
        services.AddScoped<ISpentTimeChartService, SpentTimeChartService>();
        services.AddScoped<IIssueDynamicsChartService, IssueDynamicsChartService>();

        return services;
    }

    internal static IServiceCollection AddPlanSettingsServices(this IServiceCollection services)
    {
        services.AddScoped<IPlanSettingsService, PlanSettingsService>();

        return services;
    }
}
