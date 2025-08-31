using System.Linq.Expressions;

namespace CRMService.Interfaces.Repository.Extended
{
    public interface IGetItemByPredicateRepository<TEntity> where TEntity : class
    {
        Task<TEntity?> GetItemByPredicate(Expression<Func<TEntity, bool>> predicate, bool asNoTracking = false, CancellationToken ct = default, params Expression<Func<TEntity, object>>[] includes);

        Task<List<TEntity>> GetItemsByPredicate(Expression<Func<TEntity, bool>>? predicate = null, int skip = 0, int? take = null, bool asNoTracking = false, CancellationToken ct = default, params Expression<Func<TEntity, object>>[] includes);
    }
}
