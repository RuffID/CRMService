using CRMService.Application.Abstractions.Database.Repository;
using CRMService.Application.Abstractions.Database.Repository.CrmEntity;
using CRMService.Application.Abstractions.Database.Repository.Entity;
using CRMService.Application.Abstractions.Database.Repository.Report;

namespace CRMService.Infrastructure.DataBase.Repository
{
    public class ReportsUnitOfWork(
        IUnitOfWorkScope scope,
        IEmployeePerformanceReportRepository employeePerformanceReport,
        ISpentTimeChartReportRepository spentTimeChartReport,
        IIssueDynamicsChartReportRepository issueDynamicsChartReport,
        IEmployeeRepository employee,
        IEmployeeGroupRepository employeeGroup,
        IGroupRepository group,
        IPlanRepository plan,
        IPlanSettingRepository planSetting) : MainScenarioUnitOfWork(scope), IReportsUnitOfWork
    {
        public IEmployeePerformanceReportRepository EmployeePerformanceReport { get; } = employeePerformanceReport;
        public ISpentTimeChartReportRepository SpentTimeChartReport { get; } = spentTimeChartReport;
        public IIssueDynamicsChartReportRepository IssueDynamicsChartReport { get; } = issueDynamicsChartReport;
        public IEmployeeRepository Employee { get; } = employee;
        public IEmployeeGroupRepository EmployeeGroup { get; } = employeeGroup;
        public IGroupRepository Group { get; } = group;
        public IPlanRepository Plan { get; } = plan;
        public IPlanSettingRepository PlanSetting { get; } = planSetting;
    }
}
