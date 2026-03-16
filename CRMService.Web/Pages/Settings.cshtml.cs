using CRMService.Application.Abstractions.Service;
using CRMService.Contracts.Models.Dto.Settings;
using CRMService.Contracts.Models.Responses.Results;
using CRMService.Domain.Models.Constants;
using CRMService.Web.Service.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CRMService.Web.Pages
{
    [CookieAuthorize]
    [Authorize(Roles = RolesConstants.ADMIN)]
    public class SettingsModel(IReportBackgroundService reportBackgroundService) : PageModel
    {
        public IActionResult OnGetReportBackgroundsAsync(CancellationToken ct)
        {
            return new JsonResult(reportBackgroundService.GetSettings(ct));
        }

        public async Task<IActionResult> OnPostUploadReportBackgroundAsync(IFormFile file, CancellationToken ct)
        {
            if (file is null)
                return JsonResultMapper.ToJsonResult(ServiceResult<bool>.Fail(400, "Выберите файл для загрузки."));

            await using MemoryStream stream = new();
            await file.CopyToAsync(stream, ct);

            return JsonResultMapper.ToJsonResult(await reportBackgroundService.UploadAsync(file.FileName, stream.ToArray(), ct));
        }

        public async Task<IActionResult> OnPostDeleteReportBackgroundAsync(CancellationToken ct)
        {
            return JsonResultMapper.ToJsonResult(await reportBackgroundService.DeleteAsync(ct));
        }

        public async Task<IActionResult> OnGetReportBackgroundFileAsync(CancellationToken ct)
        {
            ServiceResult<ReportBackgroundFileContentDto> result = await reportBackgroundService.GetFileContentAsync(ct);
            if (!result.Success)
                return JsonResultMapper.ToJsonResult(result);

            ReportBackgroundFileContentDto file = result.Data!;
            return File(file.Content, file.ContentType);
        }
    }
}
