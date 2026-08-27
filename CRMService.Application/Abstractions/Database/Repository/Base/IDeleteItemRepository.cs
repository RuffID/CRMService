namespace CRMService.Application.Abstractions.Database.Repository.Base
{
    public interface IDeleteItemRepository<in TEntity>
    {
        void Delete(TEntity item);
        void DeleteRange(IEnumerable<TEntity> items);
    }
}
