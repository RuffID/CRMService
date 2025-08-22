using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;

namespace CRMService.Service.DataBase
{
    public class BackupService<TContext>(TContext db, string backupFolder, ILoggerFactory logger) where TContext : DbContext
    {
        private readonly TContext _db = db;
        private readonly string _backupFolder = backupFolder;
        private readonly ILogger<BackupService<TContext>> _logger = logger.CreateLogger<BackupService<TContext>>();

        public void CreateSqlServerBackup()
        {
            try
            {
                string? connectionString = _db.Database.GetConnectionString();
                string timestamp = DateTime.Now.ToString("yyyy.MM.dd_HHmmss");
                string backupFilePath = Path.Combine(_backupFolder, $"backup_{timestamp}.sql");

                using MySqlConnection connection = new(connectionString);
                using MySqlCommand cmd = connection.CreateCommand();
                using MySqlBackup mb = new(cmd);

                connection.Open();
                mb.ExportToFile(backupFilePath);
                connection.Close();

                _logger.LogInformation("[Method:{MethodName}] Backup created at: {BackupFilePath}", nameof(CreateSqlServerBackup), backupFilePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Method:{MethodName}] Failed to create MySQL backup", nameof(CreateSqlServerBackup));
            }
        }
    }
}
