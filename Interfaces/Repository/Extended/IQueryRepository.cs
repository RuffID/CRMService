namespace CRMService.Interfaces.Repository.Extended
{
    public interface IQueryRepository<TEntity> where TEntity : class
    {
        IQueryable<TEntity> Query(bool asNoTracking = false);
    }
}
