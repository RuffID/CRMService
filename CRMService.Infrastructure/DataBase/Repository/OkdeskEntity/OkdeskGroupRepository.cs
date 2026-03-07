using CRMService.Application.Abstractions.Database.Repository.OkdeskEntity;
using CRMService.Domain.Models.OkdeskEntity;
using EFCoreLibrary.Abstractions.Database.Repository.Base;
using System.Linq.Expressions;

namespace CRMService.Infrastructure.DataBase.Repository.OkdeskEntity
{
    public class OkdeskGroupRepository(
        IGetItemByIdRepository<Group, int, OkdeskContext> getItemById,
        IGetItemByPredicateRepository<Group, OkdeskContext> getItemByPredicate) : IOkdeskGroupRepository
    {
        public Task<Group?> GetItemByIdAsync(int id, bool asNoTracking = false, Func<IQueryable<Group>, IQueryable<Group>>? include = null, CancellationToken ct = default)
            => getItemById.GetItemByIdAsync(id, asNoTracking, include, ct);

        public Task<Group?> GetItemByPredicateAsync(Expression<Func<Group, bool>> predicate, bool asNoTracking = false, Func<IQueryable<Group>, IQueryable<Group>>? include = null, CancellationToken ct = default)
            => getItemByPredicate.GetItemByPredicateAsync(predicate, asNoTracking, include, ct);

        public Task<List<Group>> GetItemsByPredicateAsync(Expression<Func<Group, bool>>? predicate = null, int skip = 0, int? take = null, bool asNoTracking = false, Func<IQueryable<Group>, IQueryable<Group>>? include = null, CancellationToken ct = default)
            => getItemByPredicate.GetItemsByPredicateAsync(predicate, skip, take, asNoTracking, include, ct);
    }
}