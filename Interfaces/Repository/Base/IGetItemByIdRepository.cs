using CRMService.Interfaces.Entity;

namespace CRMService.Interfaces.Repository.Base
{
    public interface IGetItemByIdRepository<TEntity, TId> where TEntity : class, IEntity<TId> where TId : notnull, IEquatable<TId>, IComparable<TId>
    {
        Task<TEntity?> GetItemByIdAsync(TId id, bool asNoTracking = false, Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null, CancellationToken ct = default);
    }
}