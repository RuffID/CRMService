using CRMService.Interfaces.Entity;

namespace CRMService.Interfaces.Repository.Extended
{
    public interface IUpsertItemByCodeRepository<TEntity>
        where TEntity : class, IHasCode, ICopyable<TEntity>
    {
        // Добавить/обновить по текущему Code элемента
        Task UpsertByCode(TEntity item, CancellationToken ct = default);

        // Добавить/обновить, ища по старому code (oldCode), данные берёт из item (с новым Code)
        Task UpsertByCode(string oldCode, TEntity item, CancellationToken ct = default);

        // Пакетный upsert по текущим Code элементов
        Task UpsertByCodes(IEnumerable<TEntity> items, CancellationToken ct = default);

        // Пакетный вариант: (старый code, новый элемент)
        Task UpsertByCodePairs(IEnumerable<(string OldCode, TEntity Item)> items, CancellationToken ct = default);
    }
}
