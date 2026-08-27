using CRMService.Application.Abstractions.Database.Repository;

namespace CRMService.Infrastructure.DataBase.Repository
{
    public abstract class MainScenarioUnitOfWork(IUnitOfWorkScope scope) : IUnitOfWorkScope
    {
        public Task SaveChangesAsync(CancellationToken ct = default) => scope.SaveChangesAsync(ct);

        public Task ExecuteInTransactionAsync(
            Func<CancellationToken, Task> action,
            CancellationToken ct = default) => scope.ExecuteInTransactionAsync(action, ct);
    }
}
