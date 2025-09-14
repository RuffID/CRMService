using CRMService.Interfaces.Entity;

namespace CRMService.Interfaces.Repository.Base
{
    public interface IUpsertItemByCodeRepository<TEntity> where TEntity : class, IHasCode, ICopyable<TEntity>
    {
        /*Task UpsertByCode(TEntity item, CancellationToken ct = default);

        Task UpsertByCodes(List<TEntity> items, CancellationToken ct = default);*/
    }
}