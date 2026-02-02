using System.Linq.Expressions;

namespace CRMService.Interfaces.Repository.Base
{
    public interface IGetItemByPredicateRepository<TEntity> where TEntity : class
    {
        Task<TEntity?> GetItemByPredicateAsync(Expression<Func<TEntity, bool>> predicate, bool asNoTracking = false, Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null, CancellationToken ct = default);

        Task<List<TEntity>> GetItemsByPredicateAsync(Expression<Func<TEntity, bool>>? predicate = null, int skip = 0, int? take = null, bool asNoTracking = false, Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null, CancellationToken ct = default);
    }
}