using CRMService.Application.Abstractions.Database.Repository.OkdeskEntity;
using CRMService.Domain.Models.OkdeskEntity;
using EFCoreLibrary.Abstractions.Database.Repository.Base;
using System.Linq.Expressions;

namespace CRMService.Infrastructure.DataBase.Repository.OkdeskEntity
{
    public class OkdeskKindParameterRepository(
        IGetItemByIdRepository<KindsParameter, int, OkdeskContext> getItemById,
        IGetItemByPredicateRepository<KindsParameter, OkdeskContext> getItemByPredicate) : IOkdeskKindParameterRepository
    {
        public Task<KindsParameter?> GetItemByIdAsync(int id, bool asNoTracking = false, Func<IQueryable<KindsParameter>, IQueryable<KindsParameter>>? include = null, CancellationToken ct = default)
            => getItemById.GetItemByIdAsync(id, asNoTracking, include, ct);

        public Task<KindsParameter?> GetItemByPredicateAsync(Expression<Func<KindsParameter, bool>> predicate, bool asNoTracking = false, Func<IQueryable<KindsParameter>, IQueryable<KindsParameter>>? include = null, CancellationToken ct = default)
            => getItemByPredicate.GetItemByPredicateAsync(predicate, asNoTracking, include, ct);

        public Task<List<KindsParameter>> GetItemsByPredicateAsync(Expression<Func<KindsParameter, bool>>? predicate = null, int skip = 0, int? take = null, bool asNoTracking = false, Func<IQueryable<KindsParameter>, IQueryable<KindsParameter>>? include = null, CancellationToken ct = default)
            => getItemByPredicate.GetItemsByPredicateAsync(predicate, skip, take, asNoTracking, include, ct);
    }
}