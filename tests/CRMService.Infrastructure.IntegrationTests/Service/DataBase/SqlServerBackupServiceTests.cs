using CRMService.Infrastructure.Service.DataBase;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace CRMService.Infrastructure.IntegrationTests.Service.DataBase;

public class SqlServerBackupServiceTests
{
    [Theory]
    [InlineData("CRMService")]
    [InlineData("CRM_Service-2")]
    [InlineData("Проект1")]
    public void IsValidProjectName_SafeFileName_ReturnsTrue(string projectName)
    {
        Assert.True(DatabaseBackupOptions.IsValidProjectName(projectName));
    }

    [Theory]
    [InlineData("")]
    [InlineData("CRM Service")]
    [InlineData("CRM/Service")]
    [InlineData("CRM.Service")]
    public void IsValidProjectName_EmptyOrUnsafeFileName_ReturnsFalse(string projectName)
    {
        Assert.False(DatabaseBackupOptions.IsValidProjectName(projectName));
    }

    [Theory]
    [InlineData("/var/opt/mssql/backups")]
    [InlineData("C:\\SqlBackups")]
    [InlineData("C:/SqlBackups")]
    [InlineData("\\\\server\\share")]
    public void IsAbsoluteSqlServerPath_AbsoluteProviderPath_ReturnsTrue(string path)
    {
        Assert.True(DatabaseBackupOptions.IsAbsoluteSqlServerPath(path));
    }

    [Theory]
    [InlineData("")]
    [InlineData("backups")]
    [InlineData(".\\backups")]
    [InlineData("\\server")]
    public void IsAbsoluteSqlServerPath_MissingOrRelativePath_ReturnsFalse(string path)
    {
        Assert.False(DatabaseBackupOptions.IsAbsoluteSqlServerPath(path));
    }

    [Theory]
    [InlineData("C:\\SqlBackups")]
    [InlineData("C:/SqlBackups")]
    [InlineData("\\\\server\\share")]
    public void IsAbsoluteWindowsSqlServerPath_WindowsAbsolutePath_ReturnsTrue(string path)
    {
        Assert.True(DatabaseBackupOptions.IsAbsoluteWindowsSqlServerPath(path));
    }

    [Theory]
    [InlineData("")]
    [InlineData("backups")]
    [InlineData("/var/opt/mssql/backups")]
    public void IsAbsoluteWindowsSqlServerPath_NonWindowsPath_ReturnsFalse(string path)
    {
        Assert.False(DatabaseBackupOptions.IsAbsoluteWindowsSqlServerPath(path));
    }

    [Theory]
    [InlineData("/var/opt/mssql/backups")]
    [InlineData("/")]
    public void IsAbsoluteLinuxSqlServerPath_LinuxAbsolutePath_ReturnsTrue(string path)
    {
        Assert.True(DatabaseBackupOptions.IsAbsoluteLinuxSqlServerPath(path));
    }

    [Theory]
    [InlineData("")]
    [InlineData("backups")]
    [InlineData("C:\\SqlBackups")]
    public void IsAbsoluteLinuxSqlServerPath_NonLinuxPath_ReturnsFalse(string path)
    {
        Assert.False(DatabaseBackupOptions.IsAbsoluteLinuxSqlServerPath(path));
    }

    [Fact]
    public void GetSqlServerPathForCurrentOperatingSystem_ReturnsConfiguredPath()
    {
        DatabaseBackupOptions options = new()
        {
            WindowsSqlServerPath = "C:\\SqlBackups",
            LinuxSqlServerPath = "/var/opt/mssql/backups"
        };

        string expected = OperatingSystem.IsWindows()
            ? options.WindowsSqlServerPath
            : options.LinuxSqlServerPath;

        Assert.Equal(expected, options.GetSqlServerPathForCurrentOperatingSystem());
    }

    [Fact]
    public async Task CreateBackupAsync_MissingConnectionString_FailsFast()
    {
        SqlServerBackupService service = CreateService(string.Empty, "/var/opt/mssql/backups");

        InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateBackupAsync(TestContext.Current.CancellationToken));

        Assert.Contains("connection string", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CreateBackupAsync_MissingProjectName_FailsFast()
    {
        SqlServerBackupService service = CreateService(
            "Server=localhost;Database=crm;Integrated Security=true",
            "/var/opt/mssql/backups",
            projectName: string.Empty);

        InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateBackupAsync(TestContext.Current.CancellationToken));

        Assert.Contains("project name", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CreateBackupAsync_RelativeSqlServerPath_FailsFast()
    {
        SqlServerBackupService service = CreateService(
            "Server=localhost;Database=crm;Integrated Security=true",
            "Backups");

        InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateBackupAsync(TestContext.Current.CancellationToken));

        Assert.Contains("must be absolute", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CreateBackupAsync_MissingDatabaseName_FailsFast()
    {
        SqlServerBackupService service = CreateService(
            "Server=localhost;Integrated Security=true",
            "/var/opt/mssql/backups");

        InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateBackupAsync(TestContext.Current.CancellationToken));

        Assert.Contains("does not specify a database", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CreateBackupAsync_CancelledToken_PropagatesCancellationWithoutConnection()
    {
        SqlServerBackupService service = CreateService(
            "Server=localhost;Database=crm;Integrated Security=true",
            "/var/opt/mssql/backups");
        using CancellationTokenSource cancellation = new();
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            service.CreateBackupAsync(cancellation.Token));
    }

    private static SqlServerBackupService CreateService(
        string connectionString,
        string sqlServerPath,
        string projectName = "CRMService")
    {
        DatabaseBackupOptions options = new()
        {
            ConnectionString = connectionString,
            ProjectName = projectName,
            WindowsSqlServerPath = sqlServerPath,
            LinuxSqlServerPath = sqlServerPath
        };

        return new SqlServerBackupService(
            Options.Create(options),
            NullLogger<SqlServerBackupService>.Instance);
    }
}
