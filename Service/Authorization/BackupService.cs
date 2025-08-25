using MySql.Data.MySqlClient;

namespace CRMService.Service.Authorization
{
    public class BackupService(ILoggerFactory logger)
    {
        private readonly ILogger<BackupService> _logger = logger.CreateLogger<BackupService>();

        public bool BackUpDb(string connectionString, string backupFolder)
        {
            try
            {
                string timestamp = DateTime.Now.ToString("yyyy.MM.dd_HHmmss");
                string backupFilePath = Path.Combine(backupFolder, $"backup_{timestamp}.sql");

                using MySqlConnection connection = new(connectionString);
                using MySqlCommand cmd = connection.CreateCommand();
                using MySqlBackup mb = new(cmd);

                connection.Open();
                mb.ExportToFile(backupFilePath);
                connection.Close();

                _logger.LogInformation("[Method:{MethodName}] Backup created at: {BackupFilePath}", nameof(BackUpDb), backupFilePath);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Method:{MethodName}] Failed to create MySQL backup", nameof(BackUpDb));
                return false;
            }
        }
    }
}
