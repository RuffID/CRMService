namespace CRMService.Interfaces.Repository.Base
{
    public interface IDeleteItemRepository<TEntity> where TEntity : class
    {
        void Delete(TEntity item);
    }
}
