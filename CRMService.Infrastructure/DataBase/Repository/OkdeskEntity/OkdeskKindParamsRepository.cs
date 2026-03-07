using CRMService.Application.Abstractions.Database.Repository.OkdeskEntity;
using CRMService.Domain.Models.OkdeskEntity;
using EFCoreLibrary.Abstractions.Database.Repository.Base;
using System.Linq.Expressions;

namespace CRMService.Infrastructure.DataBase.Repository.OkdeskEntity
{
    public class OkdeskKindParamsRepository(
        IGetItemByPredicateRepository<KindParam, OkdeskContext> getItemByPredicate) : IOkdeskKindParamsRepository
    {
        public Task<KindParam?> GetItemByPredicateAsync(Expression<Func<KindParam, bool>> predicate, bool asNoTracking = false, Func<IQueryable<KindParam>, IQueryable<KindParam>>? include = null, CancellationToken ct = default)
            => getItemByPredicate.GetItemByPredicateAsync(predicate, asNoTracking, include, ct);

        public Task<List<KindParam>> GetItemsByPredicateAsync(Expression<Func<KindParam, bool>>? predicate = null, int skip = 0, int? take = null, bool asNoTracking = false, Func<IQueryable<KindParam>, IQueryable<KindParam>>? include = null, CancellationToken ct = default)
            => getItemByPredicate.GetItemsByPredicateAsync(predicate, skip, take, asNoTracking, include, ct);
    }
}