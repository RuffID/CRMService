using EFCoreLibrary.Abstractions.Database.Repository.Base;
using CRMService.Domain.Models.OkdeskEntity;
using System.Linq.Expressions;
using CRMService.Application.Abstractions.Database.Repository.Entity;

namespace CRMService.Infrastructure.DataBase.Repository.Entity
{
    public class IssuePriorityRepository(
        IGetItemByIdRepository<IssuePriority, int, MainContext> getItemById,
        IGetItemByPredicateRepository<IssuePriority, MainContext> getItemByPredicate,
        ICreateItemRepository<IssuePriority, MainContext> create
    ) : IIssuePriorityRepository
    {
        public Task<IssuePriority?> GetItemByIdAsync(int id, bool asNoTracking = false, Func<IQueryable<IssuePriority>, IQueryable<IssuePriority>>? include = null, CancellationToken ct = default)
            => getItemById.GetItemByIdAsync(id, asNoTracking, include, ct);

        public Task<IssuePriority?> GetItemByPredicateAsync(Expression<Func<IssuePriority, bool>> predicate, bool asNoTracking = false, Func<IQueryable<IssuePriority>, IQueryable<IssuePriority>>? include = null, CancellationToken ct = default)
            => getItemByPredicate.GetItemByPredicateAsync(predicate, asNoTracking, include, ct);

        public Task<List<IssuePriority>> GetItemsByPredicateAsync(Expression<Func<IssuePriority, bool>>? predicate = null, int skip = 0, int? take = null, bool asNoTracking = false, Func<IQueryable<IssuePriority>, IQueryable<IssuePriority>>? include = null, CancellationToken ct = default)
            => getItemByPredicate.GetItemsByPredicateAsync(predicate, skip, take, asNoTracking, include, ct);

        public void Create(IssuePriority item)
            => create.Create(item);

        public void CreateRange(IEnumerable<IssuePriority> entities)
            => create.CreateRange(entities);
    }
}