using Microsoft.EntityFrameworkCore;

namespace CRMService.Service.DataBase
{
    public class MigrationService<TContext>(TContext db, BackupService<TContext> backupService, ILoggerFactory logger) where TContext : DbContext
    {
        private readonly TContext _db = db;
        private readonly ILogger _logger = logger.CreateLogger<MigrationService<TContext>>();
        private readonly BackupService<TContext> _backupService = backupService;

        public async Task MigrateDatabaseWithBackup()
        {           
            
            string[] pending = _db.Database.GetPendingMigrations().ToArray();
            if (pending.Length == 0)
            {
                _logger.LogInformation("[Method:{MethodName}] No pending migrations.", nameof(MigrateDatabaseWithBackup));
                return;
            }

            foreach (string m in pending)
            {
                _logger.LogInformation("[Method:{MethodName}] Pending migration: {Migration}", m, nameof(MigrateDatabaseWithBackup));
            }
                        
            _backupService.CreateSqlServerBackup();

            try
            {
                await _db.Database.MigrateAsync();
                _logger.LogInformation("[Method:{MethodName}] Migrations applied successfully.", nameof(MigrateDatabaseWithBackup));
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "[Method:{MethodName}] Migration failed. Stopping application.", nameof(MigrateDatabaseWithBackup));
                throw;
            }
        }        
    }
}
