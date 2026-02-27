/*using EFCoreLibrary.Abstractions.Database;
using EFCoreLibrary.Abstractions.Database.Repository.Base;

namespace CRMService.DataBase.Repository.Base
{
    public class DeleteItemRepository<TEntity>(IAppDbContext _context) : IDeleteItemRepository<TEntity> where TEntity : class
    {
        public virtual void Delete(TEntity entity)
        {
            _context.Set<TEntity>().Remove(entity);
        }

        public virtual void DeleteRange(IEnumerable<TEntity> entities)
        {
            _context.Set<TEntity>().RemoveRange(entities);
        }
    }
}*/