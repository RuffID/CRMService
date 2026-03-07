using CRMService.Application.Abstractions.Database.Repository.OkdeskEntity;
using EFCoreLibrary.Abstractions.Database.Repository.Base;
using CRMService.Domain.Models.OkdeskEntity;
using System.Linq.Expressions;

namespace CRMService.Infrastructure.DataBase.Repository.OkdeskEntity
{
    public class OkdeskIssuePriorityRepository(
        IGetItemByIdRepository<IssuePriority, int, OkdeskContext> getItemById,
        IGetItemByPredicateRepository<IssuePriority, OkdeskContext> getItemByPredicate) : IOkdeskIssuePriorityRepository
    {
        public Task<IssuePriority?> GetItemByIdAsync(int id, bool asNoTracking = false, Func<IQueryable<IssuePriority>, IQueryable<IssuePriority>>? include = null, CancellationToken ct = default)
            => getItemById.GetItemByIdAsync(id, asNoTracking, include, ct);

        public Task<IssuePriority?> GetItemByPredicateAsync(Expression<Func<IssuePriority, bool>> predicate, bool asNoTracking = false, Func<IQueryable<IssuePriority>, IQueryable<IssuePriority>>? include = null, CancellationToken ct = default)
            => getItemByPredicate.GetItemByPredicateAsync(predicate, asNoTracking, include, ct);

        public Task<List<IssuePriority>> GetItemsByPredicateAsync(Expression<Func<IssuePriority, bool>>? predicate = null, int skip = 0, int? take = null, bool asNoTracking = false, Func<IQueryable<IssuePriority>, IQueryable<IssuePriority>>? include = null, CancellationToken ct = default)
            => getItemByPredicate.GetItemsByPredicateAsync(predicate, skip, take, asNoTracking, include, ct);
    }
}