using CRMService.Interfaces.Entity;

namespace CRMService.Interfaces.Repository.Base
{
    public interface IUpsertItemByCodeRepository<TEntity>
        where TEntity : class, IHasCode, ICopyable<TEntity>
    {
        Task UpsertByCode(TEntity item, CancellationToken ct = default);

        Task UpsertByCode(string oldCode, TEntity item, CancellationToken ct = default);

        Task UpsertByCodes(IEnumerable<TEntity> items, CancellationToken ct = default);

        Task UpsertByCodePairs(IEnumerable<(string OldCode, TEntity Item)> items, CancellationToken ct = default);
    }
}