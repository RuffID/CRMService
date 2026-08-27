namespace CRMService.Infrastructure.DataBase.Repository
{
    public interface IMainDbContextSession
    {
        Task SaveChangesAsync(CancellationToken ct = default);

        Task ExecuteInTransactionAsync(
            Func<CancellationToken, Task> action,
            CancellationToken ct = default);
    }
}
