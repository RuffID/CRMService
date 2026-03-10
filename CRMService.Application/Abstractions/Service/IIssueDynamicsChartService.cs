using CRMService.Contracts.Models.Dto.Report;
using CRMService.Contracts.Models.Request;

namespace CRMService.Application.Abstractions.Service
{
    public interface IIssueDynamicsChartService
    {
        Task<IssueDynamicsChartDto> GetIssueDynamicsChartAsync(IssueDynamicsChartRequest request, CancellationToken ct);
    }
}