using CRMService.Application.Abstractions.Service;
using CRMService.Contracts.Models.Dto.Settings;
using CRMService.Contracts.Models.Responses.Results;

namespace CRMService.Web.Service.Settings
{
    public class ReportBackgroundService(IWebHostEnvironment environment) : IReportBackgroundService
    {
        private const int MAX_FILE_SIZE_BYTES = 10 * 1024 * 1024;

        private static readonly HashSet<string> ALLOWED_EXTENSIONS =
        [
            ".jpg",
            ".jpeg",
            ".png",
            ".webp"
        ];

        private readonly string storageRootPath = Path.Combine(environment.ContentRootPath, "Resources", "report-backgrounds");

        public ReportBackgroundDto? GetSettings(CancellationToken ct = default)
        {
            EnsureStorageCreated();

            FileInfo? file = GetBackgroundFile(ct);
            return file is null ? null : MapToDto(file);
        }

        public async Task<ServiceResult<bool>> UploadAsync(string originalFileName, byte[] content, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(originalFileName))
                return ServiceResult<bool>.Fail(400, "Выберите файл для загрузки.");

            if (content.Length == 0)
                return ServiceResult<bool>.Fail(400, "Файл пустой.");

            if (content.Length > MAX_FILE_SIZE_BYTES)
                return ServiceResult<bool>.Fail(400, "Размер изображения не должен превышать 10 МБ.");

            string extension = Path.GetExtension(originalFileName);
            if (string.IsNullOrWhiteSpace(extension) || !ALLOWED_EXTENSIONS.Contains(extension.ToLowerInvariant()))
                return ServiceResult<bool>.Fail(400, "Допустимы только изображения JPG, PNG и WEBP.");

            EnsureStorageCreated();

            await DeleteStoredBackgroundFilesAsync(ct);

            string storedFileName = $"report-background{extension.ToLowerInvariant()}";
            string filePath = Path.Combine(storageRootPath, storedFileName);

            await File.WriteAllBytesAsync(filePath, content, ct);

            return ServiceResult<bool>.Ok(true);
        }

        public Task<ServiceResult<bool>> DeleteAsync(CancellationToken ct = default)
        {
            EnsureStorageCreated();

            FileInfo? file = GetBackgroundFile(ct);
            if (file is null)
                return Task.FromResult(ServiceResult<bool>.Fail(404, "Изображение не найдено."));

            file.Delete();
            ct.ThrowIfCancellationRequested();

            return Task.FromResult(ServiceResult<bool>.Ok(true));
        }

        public async Task<ServiceResult<ReportBackgroundFileContentDto>> GetFileContentAsync(CancellationToken ct = default)
        {
            EnsureStorageCreated();

            FileInfo? file = GetBackgroundFile(ct);
            if (file is null)
                return ServiceResult<ReportBackgroundFileContentDto>.Fail(404, "Изображение не найдено.");

            byte[] content = await File.ReadAllBytesAsync(file.FullName, ct);

            return ServiceResult<ReportBackgroundFileContentDto>.Ok(new ReportBackgroundFileContentDto
            {
                FileName = file.Name,
                ContentType = GetContentType(file.Extension),
                Content = content
            });
        }

        private void EnsureStorageCreated()
        {
            Directory.CreateDirectory(storageRootPath);
        }

        private FileInfo? GetBackgroundFile(CancellationToken ct)
        {
            DirectoryInfo directory = new(storageRootPath);
            List<FileInfo> files = directory
                .EnumerateFiles()
                .Where(file => ALLOWED_EXTENSIONS.Contains(file.Extension.ToLowerInvariant()))
                .OrderByDescending(file => file.LastWriteTimeUtc)
                .ToList();

            if (files.Count <= 1)
                return files.FirstOrDefault();

            foreach (FileInfo extraFile in files.Skip(1))
            {
                extraFile.Delete();
                ct.ThrowIfCancellationRequested();
            }

            return files[0];
        }

        private Task DeleteStoredBackgroundFilesAsync(CancellationToken ct)
        {
            DirectoryInfo directory = new(storageRootPath);

            foreach (FileInfo file in directory.EnumerateFiles().Where(file => ALLOWED_EXTENSIONS.Contains(file.Extension.ToLowerInvariant())))
            {
                file.Delete();
                ct.ThrowIfCancellationRequested();
            }

            return Task.CompletedTask;
        }

        private static ReportBackgroundDto MapToDto(FileInfo file)
        {
            return new ReportBackgroundDto
            {
                DisplayName = file.Name,
                SizeBytes = file.Length,
                UploadedAtUtc = file.LastWriteTimeUtc
            };
        }

        private static string GetContentType(string extension)
        {
            string normalizedExtension = extension.ToLowerInvariant();

            return normalizedExtension switch
            {
                ".jpg" => "image/jpeg",
                ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".webp" => "image/webp",
                _ => throw new InvalidOperationException("Недопустимый формат изображения.")
            };
        }
    }
}
