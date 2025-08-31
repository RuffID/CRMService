using CRMService.Interfaces.Database;
using Microsoft.EntityFrameworkCore;

namespace CRMService.Service.DataBase
{
    public class DataBaseHandler<TContext>(IAppDbContext dbContext, ILoggerFactory logger, BackupService<TContext> backupService) where TContext : DbContext
    {
        private readonly ILogger<DataBaseHandler<TContext>> _logger = logger.CreateLogger<DataBaseHandler<TContext>>();

        public bool CheckOrUpdateDB(string ConnectionString)
        {
            if (!dbContext.Database.CanConnect())
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
            var pendingMigrations = dbContext.Database.GetPendingMigrations().ToList();

            if (pendingMigrations.Count != 0)
            {
                backupService.CreateSqlServerBackup();

                dbContext.Database.Migrate();
                _logger.LogInformation("[Method:{MethodName}] Database was updated", nameof(UpdateDB));
            }
            else
                _logger.LogInformation("[Method:{MethodName}] No changes to the database", nameof(UpdateDB));

            return true;
        }
    }
}
