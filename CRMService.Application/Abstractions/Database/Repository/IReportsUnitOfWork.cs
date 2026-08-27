using CRMService.Application.Abstractions.Database.Repository.CrmEntity;
using CRMService.Application.Abstractions.Database.Repository.Entity;
using CRMService.Application.Abstractions.Database.Repository.Report;

namespace CRMService.Application.Abstractions.Database.Repository
{
    public interface IReportsUnitOfWork : IUnitOfWorkScope
    {
        IEmployeePerformanceReportRepository EmployeePerformanceReport { get; }
        ISpentTimeChartReportRepository SpentTimeChartReport { get; }
        IIssueDynamicsChartReportRepository IssueDynamicsChartReport { get; }
        IEmployeeRepository Employee { get; }
        IEmployeeGroupRepository EmployeeGroup { get; }
        IGroupRepository Group { get; }
        IPlanRepository Plan { get; }
        IPlanSettingRepository PlanSetting { get; }
    }
}
