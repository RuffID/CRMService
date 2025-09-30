using CRMService.Interfaces.Repository.OkdeskEntity;
using CRMService.Interfaces.Repository.Base;
using CRMService.Models.OkdeskEntity;
using System.Linq.Expressions;

namespace CRMService.DataBase.Repository.Entity
{
    public class EquipmentRepository(IGetItemByIdRepository<Equipment, int> getItemById,
        ICreateItemRepository<Equipment> create,
        IUpsertItemByIdRepository<Equipment, int> upsert) : IEquipmentRepository
    {
        public Task<Equipment?> GetItemById(int id, bool asNoTracking = false, CancellationToken ct = default, params Expression<Func<Equipment, object>>[] includes)
            => getItemById.GetItemById(id, asNoTracking, ct, includes);

        public Task<List<Equipment>> GetItemsByPredicateAndSortById(Expression<Func<Equipment, bool>>? predicate = null, int skip = 0, int? take = null, bool asNoTracking = false, CancellationToken ct = default, params Expression<Func<Equipment, object>>[] includes)
            => getItemById.GetItemsByPredicateAndSortById(predicate, skip, take, asNoTracking, ct, includes);

        public void Create(Equipment item) => create.Create(item);

        public Task Upsert(Equipment item, CancellationToken ct = default) => upsert.Upsert(item, ct);

        public Task Upsert(IEnumerable<Equipment> items, CancellationToken ct = default) => upsert.Upsert(items, ct);
    }
}
