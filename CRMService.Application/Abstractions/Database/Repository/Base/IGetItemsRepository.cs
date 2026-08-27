namespace CRMService.Application.Abstractions.Database.Repository.Base
{
    public interface IGetItemsRepository<TEntity>
    {
        Task<List<TEntity>> GetItemsAsync(CancellationToken ct = default);
        Task<List<TEntity>> GetItemsReadOnlyAsync(CancellationToken ct = default);
    }
}
