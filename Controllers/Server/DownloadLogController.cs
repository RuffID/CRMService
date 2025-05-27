using CRMService.Core;
using CRMService.Interfaces.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRMService.Controllers.Server
{
    [Authorize(Roles = UserRole.ADMIN)]
    [Route("api/crm/[controller]")]
    [ApiController]
    public class DownloadLogController(IManageImage manageImage) : Controller
    {
        private readonly IManageImage _manageImage = manageImage;

        [HttpGet("today")]
        public IActionResult DownloadTodayLog(DateTime date)
        {
            string fileName = $"log_{date:yyyyMMdd}.txt";

            (FileStream? fileStream, string? contentType, string? fileName) result = _manageImage.DownloadFile("Logs", fileName);

            if (result.fileStream == null || string.IsNullOrEmpty(result.contentType) || string.IsNullOrEmpty(result.fileName))
                return NotFound("Log was not found.");

            return File(result.fileStream, result.contentType, result.fileName);
        }
    }
}
