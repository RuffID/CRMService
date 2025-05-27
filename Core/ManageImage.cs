using CRMService.Interfaces.Service;
using Microsoft.AspNetCore.StaticFiles;

namespace CRMService.Core
{
    public class ManageImage(ILoggerFactory logger) : IManageImage
    {
        private readonly ILogger<ManageImage> _logger = logger.CreateLogger<ManageImage>();

        public (FileStream? fileStream, string? contentType, string? fileName) DownloadFile(string directoryName, string fileName)
        {

            var filepath = GetFilePath(directoryName, fileName);
            if (!File.Exists(filepath))
            {
                _logger.LogInformation("[Download file method] File {fileName} not exists", fileName);
                return (null, null, null);
            }

            FileExtensionContentTypeProvider provider = new ();

            try
            {
                if (!provider.TryGetContentType(filepath, out var _ContentType))
                {
                    _ContentType = "application/octet-stream";
                }

                FileStream fileStream = new(filepath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

                return (fileStream, _ContentType, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while download file.");
                return (null, null, null);
            }
        }

        public async Task<string?> UploadFile(string directoryName, IFormFile file)
        {
            FileInfo _FileInfo = new(file.FileName);

            var filepath = GetFilePath(directoryName, _FileInfo.Name);

            try
            {
                using FileStream _FileStream = new(filepath, FileMode.Create);

                await file.CopyToAsync(_FileStream);

                return _FileInfo.Name;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while upload file.");
                return null;
            }
        }

        static string GetFilePath(string directoryName, string fileName)
        {
            var staticContentDirectory = AppContext.BaseDirectory;

            if (!Directory.Exists(directoryName))
                Directory.CreateDirectory(directoryName);

            var result = Path.Combine(staticContentDirectory, directoryName, fileName);
            return result;
        }
    }
}
