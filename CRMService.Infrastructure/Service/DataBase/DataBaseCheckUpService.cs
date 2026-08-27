using EFCoreLibrary.Abstractions.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRMService.Infrastructure.Service.DataBase
{
    public class DataBaseCheckUpService<TContext>(IAppDbContext<TContext> dbContext, ILoggerFactory logger, SqlServerBackupService backupService) where TContext : DbContext
    {
        private readonly ILogger<DataBaseCheckUpService<TContext>> _logger = logger.CreateLogger<DataBaseCheckUpService<TContext>>();

        public async Task CheckOrUpdateDBAsync(CancellationToken ct = default)
        {
            if (!await dbContext.Database.CanConnectAsync(ct))
            {
                _logger.LogError("[Method:{MethodName}] Failed to connect to the database.", nameof(CheckOrUpdateDBAsync));
                throw new InvalidOperationException("Failed to connect to the database.");
            }

            _logger.LogInformation("[Method:{MethodName}] Connection to the database was successful.", nameof(CheckOrUpdateDBAsync));

            List<string> pendingMigrations = (await dbContext.Database.GetPendingMigrationsAsync(ct)).ToList();

            if (pendingMigrations.Count != 0)
            {
                foreach (string migration in pendingMigrations)                
                    _logger.LogInformation("[Method:{MethodName}] Pending migration: {Migration}.", nameof(CheckOrUpdateDBAsync), migration);

                await backupService.CreateBackupAsync(ct);

                await dbContext.Database.MigrateAsync(ct);
                _logger.LogInformation("[Method:{MethodName}] Database was updated.", nameof(CheckOrUpdateDBAsync));
            }
            else
                _logger.LogInformation("[Method:{MethodName}] No changes to the database.", nameof(CheckOrUpdateDBAsync));
        }
    }
}
