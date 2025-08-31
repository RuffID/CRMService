namespace CRMService.Interfaces.Repository.Base
{
    public interface ICreateItemRepository<TEntity> where TEntity : class
    {
        void Create(TEntity item);
    }
}
