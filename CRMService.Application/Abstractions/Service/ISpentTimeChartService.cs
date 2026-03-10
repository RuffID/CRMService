using CRMService.Contracts.Models.Dto.Report;
using CRMService.Contracts.Models.Request;

namespace CRMService.Application.Abstractions.Service
{
    public interface ISpentTimeChartService
    {
        Task<TimeChartDto> GetSpentTimeChart(TimeChartRequest request, CancellationToken ct);
    }
}