using CRMService.Interfaces.Database;
using CRMService.Interfaces.Entity;
using CRMService.Interfaces.Repository.Extended;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace CRMService.Repository.Extended
{
    public sealed class UpsertByPredicateRepository<TEntity>(IAppDbContext _context) : IUpsertByPredicateRepository<TEntity>
        where TEntity : class, ICopyable<TEntity>
    {
        public async Task Upsert(TEntity item, Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default)
        {
            TEntity? existing = await _context.Set<TEntity>().FirstOrDefaultAsync(predicate, ct);

            if (existing is null)
            {
                _context.Set<TEntity>().Add(item);
                return;
            }

            existing.CopyData(item);
        }

        public async Task Upsert(IEnumerable<TEntity> items, Func<TEntity, Expression<Func<TEntity, bool>>> predicateFactory, CancellationToken ct = default)
        {
            foreach (TEntity item in items)
            {
                Expression<Func<TEntity, bool>> predicate = predicateFactory(item);

                TEntity? existing = await _context.Set<TEntity>().FirstOrDefaultAsync(predicate, ct);

                if (existing is null)
                    _context.Set<TEntity>().Add(item);
                else
                    existing.CopyData(item);
            }
        }
    }
}
