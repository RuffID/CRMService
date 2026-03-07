using CRMService.Application.Abstractions.Database.Repository.OkdeskEntity;
using CRMService.Domain.Models.OkdeskEntity;
using EFCoreLibrary.Abstractions.Database.Repository.Base;
using System.Linq.Expressions;

namespace CRMService.Infrastructure.DataBase.Repository.OkdeskEntity
{
    public class OkdeskKindRepository(
        IGetItemByIdRepository<Kind, int, OkdeskContext> getItemById,
        IGetItemByPredicateRepository<Kind, OkdeskContext> getItemByPredicate) : IOkdeskKindRepository
    {
        public Task<Kind?> GetItemByIdAsync(int id, bool asNoTracking = false, Func<IQueryable<Kind>, IQueryable<Kind>>? include = null, CancellationToken ct = default)
            => getItemById.GetItemByIdAsync(id, asNoTracking, include, ct);

        public Task<Kind?> GetItemByPredicateAsync(Expression<Func<Kind, bool>> predicate, bool asNoTracking = false, Func<IQueryable<Kind>, IQueryable<Kind>>? include = null, CancellationToken ct = default)
            => getItemByPredicate.GetItemByPredicateAsync(predicate, asNoTracking, include, ct);

        public Task<List<Kind>> GetItemsByPredicateAsync(Expression<Func<Kind, bool>>? predicate = null, int skip = 0, int? take = null, bool asNoTracking = false, Func<IQueryable<Kind>, IQueryable<Kind>>? include = null, CancellationToken ct = default)
            => getItemByPredicate.GetItemsByPredicateAsync(predicate, skip, take, asNoTracking, include, ct);
    }
}
