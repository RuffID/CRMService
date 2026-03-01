using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Runtime.InteropServices;

namespace CRMService.Service.DataBase
{
    public class BackupService<TContext>(string connectionString, string backupFolder, ILogger<BackupService<TContext>> logger) where TContext : DbContext
    {

        public void CreateSqlServerBackup()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                if (!Directory.Exists(backupFolder))
                    Directory.CreateDirectory(backupFolder);
            }

            string timestamp = DateTime.Now.ToString("yyyy.MM.dd_HHmmss");
            string backupFilePath = Path.Combine(backupFolder, $"backup_{timestamp}.bak");

            using SqlConnection connection = new(connectionString);
            connection.Open();

            string dbName = new SqlConnectionStringBuilder(connectionString).InitialCatalog;
            string sql = $"BACKUP DATABASE [{dbName}] TO DISK = N'{backupFilePath}' WITH FORMAT, INIT, NAME = 'Scheduled Backup';";

            using SqlCommand command = new(sql, connection);
            command.ExecuteNonQuery();

            logger.LogInformation("[Method:{MethodName}] Backup created at: {BackupFilePath}", nameof(CreateSqlServerBackup), backupFilePath);
        }
    }
}
