using System.Globalization;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CRMService.Infrastructure.Service.DataBase;

public class SqlServerBackupService(
    IOptions<DatabaseBackupOptions> options,
    ILogger<SqlServerBackupService> logger)
{
    private readonly DatabaseBackupOptions _options = options.Value;

    public async Task<string> CreateBackupAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(_options.ConnectionString))
            throw new InvalidOperationException("SQL Server backup connection string is not configured.");

        if (!DatabaseBackupOptions.IsValidProjectName(_options.ProjectName))
            throw new InvalidOperationException("SQL Server backup project name must contain only letters, digits, hyphens, or underscores.");

        if (!DatabaseBackupOptions.IsAbsoluteSqlServerPath(_options.SqlServerPath))
            throw new InvalidOperationException("SQL Server-visible backup path must be absolute.");

        SqlConnectionStringBuilder connectionString = new(_options.ConnectionString);
        if (string.IsNullOrWhiteSpace(connectionString.InitialCatalog))
            throw new InvalidOperationException("SQL Server backup connection string does not specify a database.");

        string timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss_fffffff", CultureInfo.InvariantCulture);
        string backupFileName = $"backup_{_options.ProjectName}_{timestamp}_{Guid.NewGuid():N}.bak";
        string backupFilePath = CombineSqlServerPath(_options.SqlServerPath, backupFileName);
        string escapedDatabaseName = connectionString.InitialCatalog.Replace("]", "]]", StringComparison.Ordinal);
        string escapedBackupFilePath = backupFilePath.Replace("'", "''", StringComparison.Ordinal);
        string sql = $"BACKUP DATABASE [{escapedDatabaseName}] TO DISK = N'{escapedBackupFilePath}' WITH FORMAT, INIT, NAME = 'Scheduled Backup';";

        await using SqlConnection connection = new(connectionString.ConnectionString);
        await connection.OpenAsync(ct);

        await using SqlCommand command = new(sql, connection);
        await command.ExecuteNonQueryAsync(ct);

        logger.LogInformation(
            "[Method:{MethodName}] Backup created at SQL Server path: {BackupFilePath}",
            nameof(CreateBackupAsync),
            backupFilePath);

        return backupFilePath;
    }

    private static string CombineSqlServerPath(string directory, string fileName)
    {
        string normalizedDirectory = directory.Trim();
        string trimmedDirectory = normalizedDirectory.TrimEnd('/', '\\');

        if (trimmedDirectory.Length == 0)
        {
            if (normalizedDirectory.StartsWith("/", StringComparison.Ordinal))
                return $"/{fileName}";

            throw new InvalidOperationException("SQL Server-visible backup path must contain a directory.");
        }

        char separator = normalizedDirectory.Contains('\\') && !normalizedDirectory.Contains('/') ? '\\' : '/';
        return $"{trimmedDirectory}{separator}{fileName}";
    }
}
