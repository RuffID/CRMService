using CRMService.Application.Abstractions.Database.Repository.OkdeskEntity;
using CRMService.Domain.Models.OkdeskEntity;
using EFCoreLibrary.Abstractions.Database.Repository.Base;
using System.Linq.Expressions;

namespace CRMService.Infrastructure.DataBase.Repository.OkdeskEntity
{
    public class OkdeskCompanyRepository(
        IGetItemByIdRepository<Company, int, OkdeskContext> getItemById,
        IGetItemByPredicateRepository<Company, OkdeskContext> getItemByPredicate) : IOkdeskCompanyRepository
    {
        public Task<Company?> GetItemByIdAsync(int id, bool asNoTracking = false, Func<IQueryable<Company>, IQueryable<Company>>? include = null, CancellationToken ct = default)
            => getItemById.GetItemByIdAsync(id, asNoTracking, include, ct);

        public Task<Company?> GetItemByPredicateAsync(Expression<Func<Company, bool>> predicate, bool asNoTracking = false, Func<IQueryable<Company>, IQueryable<Company>>? include = null, CancellationToken ct = default)
            => getItemByPredicate.GetItemByPredicateAsync(predicate, asNoTracking, include, ct);

        public Task<List<Company>> GetItemsByPredicateAsync(Expression<Func<Company, bool>>? predicate = null, int skip = 0, int? take = null, bool asNoTracking = false, Func<IQueryable<Company>, IQueryable<Company>>? include = null, CancellationToken ct = default)
            => getItemByPredicate.GetItemsByPredicateAsync(predicate, skip, take, asNoTracking, include, ct);
    }
}
