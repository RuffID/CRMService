using EFCoreLibrary.Abstractions.Database;
using Microsoft.EntityFrameworkCore.Storage;

namespace CRMService.Infrastructure.DataBase.Repository
{
    public class MainDbContextSession(IAppDbContext<MainContext> context) : IMainDbContextSession
    {
        public Task SaveChangesAsync(CancellationToken ct = default) => context.SaveChanges(ct);

        public async Task ExecuteInTransactionAsync(
            Func<CancellationToken, Task> action,
            CancellationToken ct = default)
        {
            await using IDbContextTransaction transaction = await context.Database.BeginTransactionAsync(ct);

            try
            {
                await action(ct);
                await SaveChangesAsync(ct);
                await transaction.CommitAsync(ct);
            }
            catch
            {
                await transaction.RollbackAsync(ct);
                throw;
            }
        }
    }
}
