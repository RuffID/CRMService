using CRMService.Abstractions.Entity;

namespace CRMService.Abstractions.Database.Repository.Base
{
    public interface IUpsertItemByIdRepository<TEntity, TId>
        where TEntity : class, IEntity<TId>, ICopyable<TEntity>
        where TId : notnull, IEquatable<TId>
    {
        Task Upsert(TEntity item, CancellationToken ct = default);
        Task Upsert(IEnumerable<TEntity> items, CancellationToken ct = default);
    }
}
