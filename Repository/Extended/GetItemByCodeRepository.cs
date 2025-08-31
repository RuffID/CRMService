using CRMService.Interfaces.Database;
using CRMService.Interfaces.Entity;
using CRMService.Interfaces.Repository.Extended;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace CRMService.Repository.Extended
{
    public class GetItemByCodeRepository<TEntity>(IAppDbContext _context) : IGetItemByCodeRepository<TEntity> where TEntity : class, IHasCode
    {
        public async Task<TEntity?> GetItemByCode(string code, bool asNoTracking = false, CancellationToken ct = default, params Expression<Func<TEntity, object>>[] includes)
        {
            IQueryable<TEntity> query = _context.Set<TEntity>();

            if (asNoTracking)
                query = query.AsNoTracking();

            foreach (var include in includes)
                query = query.Include(include);

            return await query.FirstOrDefaultAsync(x => x.Code == code, ct);
        }
    }
}