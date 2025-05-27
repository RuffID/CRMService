using CRMService.Core;
using CRMService.Interfaces.Repository;
using CRMService.Interfaces.Service;
using CRMService.Models.ConfigClass;
using CRMService.Models.Server;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace CRMService.Controllers.Server
{
    [Authorize]
    [Route("api/crm/[controller]")]
    [ApiController]
    public class UpdateController(IManageImage manageImage, IUnitOfWorkServerInfo unitOfWork, IOptions<DatabaseSettings> dbSettings, ILoggerFactory logger) : Controller
    {
        private readonly ILogger _logger = logger.CreateLogger<UpdateController>();

        [HttpPost("upload"), Authorize(Roles = UserRole.ADMIN)]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            if (!Directory.Exists("Update"))
                Directory.CreateDirectory("Update");

            var fileName = await manageImage.UploadFile("Update", file);

            if (string.IsNullOrEmpty(fileName))
                return StatusCode(500, "Error uploading file to server.");

            return Ok($"File: {fileName} - has been successfully uploaded to the server.");
        }

        [HttpGet, AllowAnonymous]
        public IActionResult DownloadFile()
        {
            string fileName = "AqbaApp.exe";
            string directory = "Update";

            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            (FileStream? fileStream, string? contentType, string? name) = manageImage.DownloadFile(directory, fileName);         

            if (fileStream == null || string.IsNullOrEmpty(contentType) || string.IsNullOrEmpty(name))
            {
                _logger.LogInformation("[Download update method] Error downloading file client. Content type: {ContentType}, file name: {FileName}, filestream: {FileStream}", contentType, name, fileStream?.ToString());

                return StatusCode(500, "Error downloading client app file.");
            }

            return File(fileStream, contentType, name);
        }

        [HttpGet("version"), AllowAnonymous]
        public async Task<IActionResult> GetVersion()
        {
            ClientAppInfo? version = await unitOfWork.ClientInfo.GetLatestReleaseInfo();
            if (version == null)
                return StatusCode(500, "An internal error occurred while retrieving the client application version.");

            return Ok(version);
        }

        [HttpPost("create_version"), Authorize(Roles = UserRole.ADMIN)]
        public async Task<IActionResult> CreateNewVersion([FromBody] ClientAppInfo info)
        {
            if (string.IsNullOrEmpty(info.Version) || info.ReleaseDate == DateTime.MinValue)
                return BadRequest("Required fields are not filled in.");

            ClientAppInfo? infoFromDb = await unitOfWork.ClientInfo.GetItem(new() { Version = info.Version });

            if (infoFromDb != null)
                return Conflict("The version already exists.");

            unitOfWork.ClientInfo.Create(info);

            await unitOfWork.SaveAsync();

            return NoContent();
        }

        [HttpDelete("version"), Authorize(Roles = UserRole.ADMIN)]
        public async Task<IActionResult> DeleteVersion([FromQuery] string version)
        {
            if (string.IsNullOrEmpty(version))
                return BadRequest("Required fields are not filled in.");

            ClientAppInfo? info = await unitOfWork.ClientInfo.GetItem(new () { Version = version });

            if (info == null)
                return NotFound("Version not found.");

            unitOfWork.ClientInfo.Delete(info);

            await unitOfWork.SaveAsync();

            return NoContent();
        }

        [HttpGet("versions")]
        public async Task<IActionResult> GetCategories([FromQuery] int startIndex = 0)
        {
            IEnumerable<ClientAppInfo>? versions = await unitOfWork.ClientInfo.GetItems(startIndex, dbSettings.Value.LimitForRetrievingEntitiesFromDb);

            if (versions == null || !versions.Any())
                return NotFound();

            return Ok(versions);
        }
    }
}
