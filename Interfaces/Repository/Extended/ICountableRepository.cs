using System.Linq.Expressions;

namespace CRMService.Interfaces.Repository.Extended
{
    public interface ICountItemRepository<TEntity> where TEntity : class
    {
        Task<int> GetCountOfItems(Expression<Func<TEntity, bool>>? predicate = null, CancellationToken ct = default);
    }
}
