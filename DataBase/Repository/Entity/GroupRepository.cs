using CRMService.Interfaces.Repository.Base;
using CRMService.Interfaces.Repository.OkdeskEntity;
using CRMService.Models.OkdeskEntity;
using System.Linq.Expressions;

namespace CRMService.DataBase.Repository.Entity
{
    public class GroupRepository(IGetItemByIdRepository<Group, int> getItemByid,
        IUpsertItemByIdRepository<Group, int> upsertItemByid,
        ICreateItemRepository<Group> create) : IGroupRepository
    {
        public Task<Group?> GetItemById(int id, bool asNoTracking = false, CancellationToken ct = default, params Expression<Func<Group, object>>[] includes)
            => getItemByid.GetItemById(id, asNoTracking, ct, includes);

        public Task<List<Group>> GetItemsByPredicateAndSortById(Expression<Func<Group, bool>>? predicate = null, int skip = 0, int? take = null, bool asNoTracking = false, CancellationToken ct = default, params Expression<Func<Group, object>>[] includes)
            => getItemByid.GetItemsByPredicateAndSortById(predicate, skip, take, asNoTracking, ct, includes);

        public void Create(Group item) => create.Create(item);

        public Task Upsert(Group item, CancellationToken ct = default) => upsertItemByid.Upsert(item, ct);

        public Task Upsert(IEnumerable<Group> items, CancellationToken ct = default) => upsertItemByid.Upsert(items, ct);
    }
}
