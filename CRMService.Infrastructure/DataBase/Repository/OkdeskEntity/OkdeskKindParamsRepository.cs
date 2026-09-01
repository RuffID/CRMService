using CRMService.Application.Abstractions.Database.Repository.OkdeskEntity;
using CRMService.Application.Models.OkdeskSource;
using EFCoreLibrary.Abstractions.Database.Repository.Base;
using System.Linq.Expressions;

namespace CRMService.Infrastructure.DataBase.Repository.OkdeskEntity
{
    public partial class OkdeskKindParamsRepository(
        IGetItemByPredicateRepository<OkdeskKindParameterConnectionRecord, OkdeskContext> getItemByPredicate) : IOkdeskKindParamsRepository
    {
        public Task<OkdeskKindParameterConnectionRecord?> GetItemByPredicateAsync(Expression<Func<OkdeskKindParameterConnectionRecord, bool>> predicate, bool asNoTracking = false, Func<IQueryable<OkdeskKindParameterConnectionRecord>, IQueryable<OkdeskKindParameterConnectionRecord>>? include = null, CancellationToken ct = default)
            => getItemByPredicate.GetItemByPredicateAsync(predicate, asNoTracking, include, ct);

        public Task<List<OkdeskKindParameterConnectionRecord>> GetItemsByPredicateAsync(Expression<Func<OkdeskKindParameterConnectionRecord, bool>>? predicate = null, int skip = 0, int? take = null, bool asNoTracking = false, Func<IQueryable<OkdeskKindParameterConnectionRecord>, IQueryable<OkdeskKindParameterConnectionRecord>>? include = null, CancellationToken ct = default)
            => getItemByPredicate.GetItemsByPredicateAsync(predicate, skip, take, asNoTracking, include, ct);
    }
}
