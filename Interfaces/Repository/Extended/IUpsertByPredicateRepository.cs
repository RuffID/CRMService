using CRMService.Interfaces.Entity;
using System.Linq.Expressions;

namespace CRMService.Interfaces.Repository.Extended
{
    public interface IUpsertByPredicateRepository<TEntity>
        where TEntity : class, ICopyable<TEntity>
    {
        Task Upsert(TEntity item, Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default);

        Task Upsert(IEnumerable<TEntity> items, Func<TEntity, Expression<Func<TEntity, bool>>> predicateFactory, CancellationToken ct = default);
    }
}
