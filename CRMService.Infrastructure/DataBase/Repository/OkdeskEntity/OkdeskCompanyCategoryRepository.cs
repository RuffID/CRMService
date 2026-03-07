using CRMService.Application.Abstractions.Database.Repository.OkdeskEntity;
using CRMService.Domain.Models.OkdeskEntity;
using EFCoreLibrary.Abstractions.Database.Repository.Base;
using System.Linq.Expressions;

namespace CRMService.Infrastructure.DataBase.Repository.OkdeskEntity
{
    public class OkdeskCompanyCategoryRepository(
        IGetItemByIdRepository<CompanyCategory, int, OkdeskContext> getItemById,
        IGetItemByPredicateRepository<CompanyCategory, OkdeskContext> getItemByPredicate) : IOkdeskCompanyCategoryRepository
    {
        public Task<CompanyCategory?> GetItemByIdAsync(int id, bool asNoTracking = false, Func<IQueryable<CompanyCategory>, IQueryable<CompanyCategory>>? include = null, CancellationToken ct = default)
            => getItemById.GetItemByIdAsync(id, asNoTracking, include, ct);

        public Task<CompanyCategory?> GetItemByPredicateAsync(Expression<Func<CompanyCategory, bool>> predicate, bool asNoTracking = false, Func<IQueryable<CompanyCategory>, IQueryable<CompanyCategory>>? include = null, CancellationToken ct = default)
            => getItemByPredicate.GetItemByPredicateAsync(predicate, asNoTracking, include, ct);

        public Task<List<CompanyCategory>> GetItemsByPredicateAsync(Expression<Func<CompanyCategory, bool>>? predicate = null, int skip = 0, int? take = null, bool asNoTracking = false, Func<IQueryable<CompanyCategory>, IQueryable<CompanyCategory>>? include = null, CancellationToken ct = default)
            => getItemByPredicate.GetItemsByPredicateAsync(predicate, skip, take, asNoTracking, include, ct);
    }
}
