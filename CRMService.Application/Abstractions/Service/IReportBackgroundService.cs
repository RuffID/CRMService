using CRMService.Contracts.Models.Dto.Settings;
using CRMService.Contracts.Models.Responses.Results;

namespace CRMService.Application.Abstractions.Service
{
    public interface IReportBackgroundService
    {
        ReportBackgroundDto? GetSettings(CancellationToken ct = default);

        Task<ServiceResult<bool>> UploadAsync(string originalFileName, byte[] content, CancellationToken ct = default);

        Task<ServiceResult<bool>> DeleteAsync(CancellationToken ct = default);

        Task<ServiceResult<ReportBackgroundFileContentDto>> GetFileContentAsync(CancellationToken ct = default);
    }
}
