using CRMService.Application.Models.Report;
using CRMService.Contracts.Models.Request;

namespace CRMService.Application.Abstractions.Database.Repository.Report
{
    public interface IReportRepository
    {
        Task<List<SolvedIssuesCountInfo>> GetOpenIssuesCountByEmployees(ReportRequest? filters, CancellationToken ct);
        Task<List<SolvedIssuesCountInfo>> GetSolvedIssuesCountByEmployees(DateTime dateFrom, DateTime dateTo, ReportRequest? filters, CancellationToken ct);
        Task<List<IssueInfo>> GetSolvedIssuesByEmployee(DateTime dateFrom, DateTime dateTo, ReportRequest? filters, CancellationToken ct);
        Task<List<SpentedTimeInfo>> GetSpentedTimeByEmployee(DateTime dateFrom, DateTime dateTo, ReportRequest? filters, CancellationToken ct);
        Task<List<TimeChartPointInfo>> GetSpentTimeChartByEmployees(DateTime dateFrom, DateTime dateTo, string timeAxis, string granularity, IReadOnlyCollection<int> employeeIds, CancellationToken ct);
        Task<List<TimeChartPointInfo>> GetSpentTimeChartByGroups(DateTime dateFrom, DateTime dateTo, string timeAxis, string granularity, IReadOnlyCollection<int> groupIds, CancellationToken ct);
    }
}