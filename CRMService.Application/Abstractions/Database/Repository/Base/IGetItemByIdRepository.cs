namespace CRMService.Application.Abstractions.Database.Repository.Base
{
    public interface IGetItemByIdRepository<TEntity, in TId>
    {
        Task<TEntity?> GetItemByIdAsync(TId id, CancellationToken ct = default);
        Task<TEntity?> GetItemByIdReadOnlyAsync(TId id, CancellationToken ct = default);
    }
}
