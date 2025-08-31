using CRMService.Interfaces.Repository.Base;
using CRMService.Interfaces.Repository.Entity;
using CRMService.Models.Entity;
using System.Linq.Expressions;

namespace CRMService.Repository.Entity
{
    public class GroupRepository(IGetItemByIdRepository<Group, int> _get,
        IUpsertItemByIdRepository<Group, int> _upsert,
        ICreateItemRepository<Group> _create) : IGroupRepository
    {
        public Task<Group?> GetItemById(int id, bool asNoTracking = false, CancellationToken ct = default, params Expression<Func<Group, object>>[] includes)
            => _get.GetItemById(id, asNoTracking, ct, includes);

        public Task<List<Group>> GetItemsByPredicateAndSortById(Expression<Func<Group, bool>>? predicate = null, int skip = 0, int? take = null, bool asNoTracking = false, CancellationToken ct = default, params Expression<Func<Group, object>>[] includes)
            => _get.GetItemsByPredicateAndSortById(predicate, skip, take, asNoTracking, ct, includes);

        public void Create(Group item) => _create.Create(item);

        public Task Upsert(Group item, CancellationToken ct = default) => _upsert.Upsert(item, ct);

        public Task Upsert(IEnumerable<Group> items, CancellationToken ct = default) => _upsert.Upsert(items, ct);
    }
}
