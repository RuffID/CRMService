using CRMService.DataBase;
using Microsoft.EntityFrameworkCore;

namespace CRMService.Service.Authorization
{
    public class DataBaseHandler(CrmAuthorizationContext _dbContext, ILoggerFactory _logger, BackupService _backupService)
    {
        private readonly ILogger<DataBaseHandler> _logger = _logger.CreateLogger<DataBaseHandler>();

        public bool CheckOrUpdateDB(string ConnectionString)
        {
            if (!_dbContext.Database.CanConnect())
            {
                _logger.LogError("[Method:{MethodName}] Failed to connect to the database\r\nConnection string: \"{ConnectionString}\"\r\nServer startup has been stopped!", nameof(CheckOrUpdateDB), ConnectionString);
                return false;
            }
            _logger.LogInformation($"[Method:{nameof(CheckOrUpdateDB)}] Connection to the database was successful");

            try
            {
                if (!UpdateDB(ConnectionString))
                    return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Method:{MethodName}] Error update DB ", nameof(CheckOrUpdateDB));
                return false;
            }

            return true;
        }

        private bool UpdateDB(string connectionString)
        {
            var pendingMigrations = _dbContext.Database.GetPendingMigrations().ToList();

            if (pendingMigrations.Count != 0)
            {
                string backupPath ="/backups";

                if (!_backupService.BackUpDb(connectionString, backupPath))
                    return false;

                _dbContext.Database.Migrate();
                _logger.LogInformation("[Method:{MethodName}] Database was updated", nameof(UpdateDB));
            }
            else
                _logger.LogInformation("[Method:{MethodName}] No changes to the database", nameof(UpdateDB));

            return true;
        }
    }
}
