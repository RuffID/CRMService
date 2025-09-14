using CRMService.Interfaces.Database;
using CRMService.Interfaces.Repository.Base;

namespace CRMService.DataBase.Repository.Base
{
    public class CreateItemRepository<TEntity>(IAppDbContext _context) : ICreateItemRepository<TEntity> where TEntity : class
    {
        public virtual void Create(TEntity entity)
        {
            _context.Set<TEntity>().Add(entity);
        }
    }
}
