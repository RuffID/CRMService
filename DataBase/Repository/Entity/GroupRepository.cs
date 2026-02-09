using CRMService.Abstractions.Database.Repository.Base;
using CRMService.Abstractions.Database.Repository.OkdeskEntity;
using CRMService.Models.OkdeskEntity;
using System.Linq.Expressions;

namespace CRMService.DataBase.Repository.Entity
{
    public class GroupRepository(IGetItemByIdRepository<Group, int> getItemByid,
        IGetItemByPredicateRepository<Group> getItemByPredicate,
        ICreateItemRepository<Group> create) : IGroupRepository
    {
        public Task<Group?> GetItemByIdAsync(int id, bool asNoTracking = false, Func<IQueryable<Group>, IQueryable<Group>>? include = null, CancellationToken ct = default)
            => getItemByid.GetItemByIdAsync(id, asNoTracking, include, ct);

        public Task<Group?> GetItemByPredicateAsync(Expression<Func<Group, bool>> predicate, bool asNoTracking = false, Func<IQueryable<Group>, IQueryable<Group>>? include = null, CancellationToken ct = default)
            => getItemByPredicate.GetItemByPredicateAsync(predicate, asNoTracking, include, ct);

        public Task<List<Group>> GetItemsByPredicateAsync(Expression<Func<Group, bool>>? predicate = null, int skip = 0, int? take = null, bool asNoTracking = false, Func<IQueryable<Group>, IQueryable<Group>>? include = null, CancellationToken ct = default)
            => getItemByPredicate.GetItemsByPredicateAsync(predicate, skip, take, asNoTracking, include, ct);

        public void Create(Group item) => create.Create(item);

        public void CreateRange(IEnumerable<Group> entities) => create.CreateRange(entities);
    }
}
