using CRMService.Models.Report;
using CRMService.Models.Request;

namespace CRMService.Abstractions.Service
{
    public interface IReportService
    {
        Task<List<ReportInfo>> GetFullReportOnEmployees(DateTime dateFrom, DateTime dateTo, ReportRequest filters, CancellationToken ct);
    }
}
