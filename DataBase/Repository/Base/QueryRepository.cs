/*using EFCoreLibrary.Abstractions.Database;
using EFCoreLibrary.Abstractions.Database.Repository.Base;
using Microsoft.EntityFrameworkCore;

namespace CRMService.DataBase.Repository.Base
{
    public class QueryRepository<TEntity>(IAppDbContext context) : IQueryRepository<TEntity> where TEntity : class
    {
        public virtual IQueryable<TEntity> Query(bool asNoTracking = false)
        {
            IQueryable<TEntity> query = context.Set<TEntity>();

            if (asNoTracking)
                query = query.AsNoTracking();

            return query;
        }
    }
}
*/