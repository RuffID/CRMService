using CRMService.Application.Models.Report;

namespace CRMService.Application.Abstractions.Database.Repository.Report
{
    public interface ISpentTimeChartReportRepository
    {
        Task<List<TimeChartPointInfo>> GetSpentTimeChartByEmployees(DateTime dateFrom, DateTime dateTo, string timeAxis, string granularity, IReadOnlyCollection<int> employeeIds, CancellationToken ct);
        Task<List<TimeChartPointInfo>> GetSpentTimeChartByGroups(DateTime dateFrom, DateTime dateTo, string timeAxis, string granularity, IReadOnlyCollection<int> groupIds, CancellationToken ct);
    }
}