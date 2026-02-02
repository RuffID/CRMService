using CRMService.Interfaces.Repository.Base;
using CRMService.Interfaces.Repository.OkdeskEntity;
using CRMService.Models.OkdeskEntity;
using System.Linq.Expressions;

namespace CRMService.DataBase.Repository.Entity
{
    public class IssuePriorityRepository(IGetItemByIdRepository<IssuePriority, int> getItemById,
        IGetItemByPredicateRepository<IssuePriority> getItemByPredicate,
        ICreateItemRepository<IssuePriority> create) : IIssuePriorityRepository
    {
        public Task<IssuePriority?> GetItemByIdAsync(int id, bool asNoTracking = false, Func<IQueryable<IssuePriority>, IQueryable<IssuePriority>>? include = null, CancellationToken ct = default)
            => getItemById.GetItemByIdAsync(id, asNoTracking, include, ct);

        public Task<IssuePriority?> GetItemByPredicateAsync(Expression<Func<IssuePriority, bool>> predicate, bool asNoTracking = false, Func<IQueryable<IssuePriority>, IQueryable<IssuePriority>>? include = null, CancellationToken ct = default)
            => getItemByPredicate.GetItemByPredicateAsync(predicate, asNoTracking, include, ct);

        public Task<List<IssuePriority>> GetItemsByPredicateAsync(Expression<Func<IssuePriority, bool>>? predicate = null, int skip = 0, int? take = null, bool asNoTracking = false, Func<IQueryable<IssuePriority>, IQueryable<IssuePriority>>? include = null, CancellationToken ct = default)
            => getItemByPredicate.GetItemsByPredicateAsync(predicate, skip, take, asNoTracking, include, ct);

        public void Create(IssuePriority item) => create.Create(item);

        public void CreateRange(IEnumerable<IssuePriority> entities) => create.CreateRange(entities);
    }
}
