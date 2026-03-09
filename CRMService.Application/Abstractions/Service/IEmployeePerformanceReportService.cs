using CRMService.Application.Models.Report;
using CRMService.Contracts.Models.Request;

namespace CRMService.Application.Abstractions.Service
{
    public interface IEmployeePerformanceReportService
    {
        Task<List<ReportInfo>> GetFullReportOnEmployees(DateTime dateFrom, DateTime dateTo, ReportRequest filters, CancellationToken ct);
    }
}
