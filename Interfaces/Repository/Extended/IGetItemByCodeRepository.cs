using CRMService.Interfaces.Entity;
using System.Linq.Expressions;

namespace CRMService.Interfaces.Repository.Extended
{
    public interface IGetItemByCodeRepository<TEntity> where TEntity : class, IHasCode
    {
        Task<TEntity?> GetItemByCode(
            string code,
            bool asNoTracking = false,
            CancellationToken ct = default,
            params Expression<Func<TEntity, object>>[] includes);
    }
}
