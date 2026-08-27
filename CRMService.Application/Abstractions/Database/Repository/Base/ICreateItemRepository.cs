namespace CRMService.Application.Abstractions.Database.Repository.Base
{
    public interface ICreateItemRepository<in TEntity>
    {
        void Create(TEntity item);
        void CreateRange(IEnumerable<TEntity> items);
    }
}
