using CRMService.Web.Service.Settings;
using Microsoft.Extensions.FileProviders;

namespace CRMService.Web.IntegrationTests.Services;

public class ReportBackgroundServiceTests
{
    [Fact]
    public async Task UploadAndRead_UseUniqueTemporaryContentRoot()
    {
        string contentRoot = Directory.CreateTempSubdirectory("crm-web-report-").FullName;

        try
        {
            ReportBackgroundService service = new(new TestWebHostEnvironment(contentRoot));

            CRMService.Contracts.Models.Responses.Results.ServiceResult<bool> upload =
                await service.UploadAsync("background.PNG", [1, 2, 3], CancellationToken.None);
            CRMService.Contracts.Models.Responses.Results.ServiceResult<CRMService.Contracts.Models.Dto.Settings.ReportBackgroundFileContentDto> file =
                await service.GetFileContentAsync(CancellationToken.None);

            Assert.True(upload.Success);
            Assert.True(file.Success);
            Assert.Equal("report-background.png", file.Data?.FileName);
            Assert.Equal("image/png", file.Data?.ContentType);
            Assert.Equal([1, 2, 3], file.Data?.Content);
            Assert.StartsWith(
                Path.GetFullPath(contentRoot),
                Path.GetFullPath(Path.Combine(contentRoot, "Resources", "report-backgrounds", file.Data!.FileName)),
                StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            Directory.Delete(contentRoot, recursive: true);
        }

        Assert.False(Directory.Exists(contentRoot));
    }

    [Fact]
    public async Task Upload_Cancelled_PropagatesAndTemporaryRootIsCleaned()
    {
        string contentRoot = Directory.CreateTempSubdirectory("crm-web-report-cancel-").FullName;

        try
        {
            ReportBackgroundService service = new(new TestWebHostEnvironment(contentRoot));
            using CancellationTokenSource source = new();
            source.Cancel();

            await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
                service.UploadAsync("background.png", [1, 2, 3], source.Token));
        }
        finally
        {
            Directory.Delete(contentRoot, recursive: true);
        }

        Assert.False(Directory.Exists(contentRoot));
    }

    [Fact]
    public async Task Upload_InvalidFileSystemLayout_FailsFast()
    {
        string contentRoot = Directory.CreateTempSubdirectory("crm-web-report-error-").FullName;

        try
        {
            await File.WriteAllTextAsync(
                Path.Combine(contentRoot, "Resources"),
                "not a directory",
                TestContext.Current.CancellationToken);
            ReportBackgroundService service = new(new TestWebHostEnvironment(contentRoot));

            await Assert.ThrowsAnyAsync<IOException>(() =>
                service.UploadAsync("background.png", [1], CancellationToken.None));
        }
        finally
        {
            Directory.Delete(contentRoot, recursive: true);
        }
    }

    private class TestWebHostEnvironment(string contentRootPath) : IWebHostEnvironment
    {
        public string ApplicationName { get; set; } = "CRMService.Web.IntegrationTests";
        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
        public string WebRootPath { get; set; } = contentRootPath;
        public string EnvironmentName { get; set; } = "Testing";
        public string ContentRootPath { get; set; } = contentRootPath;
        public IFileProvider ContentRootFileProvider { get; set; } = new PhysicalFileProvider(contentRootPath);
    }
}
