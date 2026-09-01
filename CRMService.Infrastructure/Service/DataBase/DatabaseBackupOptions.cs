namespace CRMService.Infrastructure.Service.DataBase;

public class DatabaseBackupOptions
{
    public const string SECTION_NAME = "DatabaseBackup";

    public string ConnectionString { get; set; } = string.Empty;

    public string ProjectName { get; set; } = string.Empty;

    public string WindowsSqlServerPath { get; set; } = string.Empty;

    public string LinuxSqlServerPath { get; set; } = string.Empty;

    public string GetSqlServerPathForCurrentOperatingSystem()
    {
        if (OperatingSystem.IsWindows())
            return WindowsSqlServerPath;

        if (OperatingSystem.IsLinux())
            return LinuxSqlServerPath;

        throw new PlatformNotSupportedException("SQL Server backup is supported only on Windows and Linux.");
    }

    public static bool IsValidProjectName(string? projectName)
    {
        if (string.IsNullOrWhiteSpace(projectName))
            return false;

        return projectName.All(character =>
            char.IsLetterOrDigit(character) || character is '-' or '_');
    }

    public static bool IsAbsoluteSqlServerPath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return false;

        string value = path.Trim();

        if (value.StartsWith("/", StringComparison.Ordinal))
            return true;

        if (value.Length >= 3
            && char.IsAsciiLetter(value[0])
            && value[1] == ':'
            && (value[2] == '\\' || value[2] == '/'))
        {
            return true;
        }

        if (!value.StartsWith("\\\\", StringComparison.Ordinal))
            return false;

        string[] parts = value[2..].Split('\\', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length >= 2;
    }

    public static bool IsAbsoluteWindowsSqlServerPath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return false;

        string value = path.Trim();

        if (value.Length >= 3
            && char.IsAsciiLetter(value[0])
            && value[1] == ':'
            && (value[2] == '\\' || value[2] == '/'))
        {
            return true;
        }

        if (!value.StartsWith("\\\\", StringComparison.Ordinal))
            return false;

        string[] parts = value[2..].Split('\\', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length >= 2;
    }

    public static bool IsAbsoluteLinuxSqlServerPath(string? path) =>
        !string.IsNullOrWhiteSpace(path)
        && path.Trim().StartsWith("/", StringComparison.Ordinal);
}
