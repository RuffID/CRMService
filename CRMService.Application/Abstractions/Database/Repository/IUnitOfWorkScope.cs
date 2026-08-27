namespace CRMService.Application.Abstractions.Database.Repository
{
    public interface IUnitOfWorkScope
    {
        Task SaveChangesAsync(CancellationToken ct = default);

        Task ExecuteInTransactionAsync(
            Func<CancellationToken, Task> action,
            CancellationToken ct = default);
    }
}
