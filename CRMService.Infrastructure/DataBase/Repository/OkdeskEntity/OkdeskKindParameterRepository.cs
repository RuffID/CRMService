using CRMService.Application.Abstractions.Database.Repository.OkdeskEntity;
using CRMService.Application.Models.OkdeskSource;
using EFCoreLibrary.Abstractions.Database.Repository.Base;
using System.Linq.Expressions;

namespace CRMService.Infrastructure.DataBase.Repository.OkdeskEntity
{
    public partial class OkdeskKindParameterRepository(
        IGetItemByPredicateRepository<OkdeskKindParameterRecord, OkdeskContext> getItemByPredicate) : IOkdeskKindParameterRepository
    {
        public Task<OkdeskKindParameterRecord?> GetItemByPredicateAsync(Expression<Func<OkdeskKindParameterRecord, bool>> predicate, bool asNoTracking = false, Func<IQueryable<OkdeskKindParameterRecord>, IQueryable<OkdeskKindParameterRecord>>? include = null, CancellationToken ct = default)
            => getItemByPredicate.GetItemByPredicateAsync(predicate, asNoTracking, include, ct);

        public Task<List<OkdeskKindParameterRecord>> GetItemsByPredicateAsync(Expression<Func<OkdeskKindParameterRecord, bool>>? predicate = null, int skip = 0, int? take = null, bool asNoTracking = false, Func<IQueryable<OkdeskKindParameterRecord>, IQueryable<OkdeskKindParameterRecord>>? include = null, CancellationToken ct = default)
            => getItemByPredicate.GetItemsByPredicateAsync(predicate, skip, take, asNoTracking, include, ct);
    }
}
