using CRMService.Abstractions.Entity;
using System.Linq.Expressions;

namespace CRMService.Abstractions.Database.Repository.Base
{
    public interface IUpsertItemByPredicateRepository<TEntity>
        where TEntity : class, ICopyable<TEntity>
    {
        Task Upsert(TEntity item, Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default);

        Task Upsert(IEnumerable<TEntity> items, Func<TEntity, Expression<Func<TEntity, bool>>> predicateFactory, CancellationToken ct = default);
    }
}
