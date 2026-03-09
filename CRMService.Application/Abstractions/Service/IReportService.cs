using CRMService.Application.Models.Report;
using CRMService.Contracts.Models.Dto.Report;
using CRMService.Contracts.Models.Request;

namespace CRMService.Application.Abstractions.Service
{
    public interface IReportService
    {
        Task<List<ReportInfo>> GetFullReportOnEmployees(DateTime dateFrom, DateTime dateTo, ReportRequest filters, CancellationToken ct);
        Task<TimeChartDto> GetSpentTimeChart(TimeChartRequest request, CancellationToken ct);
    }
}