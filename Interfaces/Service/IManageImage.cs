namespace CRMService.Interfaces.Service
{
    public interface IManageImage
    {
        Task<string?> UploadFile(string directoryName, IFormFile _IFormFile);
        (FileStream? fileStream, string? contentType, string? fileName) DownloadFile(string directoryName, string fileName);
    }
}