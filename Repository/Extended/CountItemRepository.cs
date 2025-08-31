using CRMService.Interfaces.Database;
using CRMService.Interfaces.Repository.Extended;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace CRMService.Repository.Extended
{
    public class CountItemRepository<TEntity>(IAppDbContext _context) : ICountItemRepository<TEntity> where TEntity : class
    {
        public Task<int> GetCountOfItems(
            Expression<Func<TEntity, bool>>? predicate = null,
            CancellationToken ct = default)
        {
            IQueryable<TEntity> query = _context.Set<TEntity>().AsNoTracking();

            if (predicate != null)
                query = query.Where(predicate);

            return query.CountAsync(ct);
        }
    }
}
