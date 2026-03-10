using CRMService.Application.Models.Report;

namespace CRMService.Application.Abstractions.Database.Repository.Report
{
    public interface IIssueDynamicsChartReportRepository
    {
        Task<List<IssueDynamicsPointInfo>> GetCreatedIssuesAsync(DateTime dateFrom, DateTime dateTo, string granularity, CancellationToken ct);
        Task<List<IssueDynamicsPointInfo>> GetCompletedIssuesAsync(DateTime dateFrom, DateTime dateTo, string granularity, CancellationToken ct);
    }
}