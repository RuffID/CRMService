using CRMService.Models.Report;
using CRMService.Models.Request;

namespace CRMService.Abstractions.Database.Repository.Report
{
    public interface IReportRepository
    {
        Task<List<IssueInfo>> GetInfoForOpenIssuesByEmployee(ReportRequest? filters, CancellationToken ct);
        Task<List<SolvedIssuesCountInfo>> GetSolvedIssuesCountByEmployees(DateTime dateFrom, DateTime dateTo, ReportRequest? filters, CancellationToken ct);
        Task<List<IssueInfo>> GetSolvedIssuesByEmployee(DateTime dateFrom, DateTime dateTo, ReportRequest? filters, CancellationToken ct);
        Task<List<SpentedTimeInfo>> GetSpentedTimeByEmployee(DateTime dateFrom, DateTime dateTo, ReportRequest? filters, CancellationToken ct);
    }
}
