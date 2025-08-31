using CRMService.Interfaces.Database;
using CRMService.Interfaces.Repository.Base;

namespace CRMService.Repository.Base
{
    public class DeleteItemRepository<TEntity>(IAppDbContext _context) : IDeleteItemRepository<TEntity> where TEntity : class
    {
        public virtual void Delete(TEntity entity)
        {
            _context.Set<TEntity>().Remove(entity);
        }
    }
}