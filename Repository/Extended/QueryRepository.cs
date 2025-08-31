using CRMService.Interfaces.Database;
using CRMService.Interfaces.Repository.Extended;
using Microsoft.EntityFrameworkCore;

namespace CRMService.Repository.Extended
{
    public class QueryRepository<TEntity>(IAppDbContext _context) : IQueryRepository<TEntity> where TEntity : class
    {
        public virtual IQueryable<TEntity> Query(bool asNoTracking = false)
        {
            IQueryable<TEntity> query = _context.Set<TEntity>();

            if (asNoTracking)
                query = query.AsNoTracking();

            return query;
        }
    }
}
