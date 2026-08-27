using CRMService.Application.Abstractions.Database.Repository;

namespace CRMService.Infrastructure.DataBase.Repository
{
    public class MainDbUnitOfWorkScope(IMainDbContextSession session) : IUnitOfWorkScope
    {
        public Task SaveChangesAsync(CancellationToken ct = default) => session.SaveChangesAsync(ct);

        public Task ExecuteInTransactionAsync(
            Func<CancellationToken, Task> action,
            CancellationToken ct = default) => session.ExecuteInTransactionAsync(action, ct);
    }
}
