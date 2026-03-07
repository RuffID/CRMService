using EFCoreLibrary.Abstractions.Database.Repository.Base;
using CRMService.Application.Abstractions.Database.Repository.OkdeskEntity;
using CRMService.Domain.Models.OkdeskEntity;
using System.Linq.Expressions;

namespace CRMService.Infrastructure.DataBase.Repository.Entity
{
    public class CategoryRepository(
        IGetItemByIdRepository<CompanyCategory, int, MainContext> getItemById,
        ICreateItemRepository<CompanyCategory, MainContext> create,
        IGetItemByPredicateRepository<CompanyCategory, MainContext> getItemByPredicate
    ) : ICompanyCategoryRepository
    {
        public Task<CompanyCategory?> GetItemByIdAsync(int id, bool asNoTracking = false, Func<IQueryable<CompanyCategory>, IQueryable<CompanyCategory>>? include = null, CancellationToken ct = default)
            => getItemById.GetItemByIdAsync(id, asNoTracking, include, ct);

        public Task<CompanyCategory?> GetItemByPredicateAsync(Expression<Func<CompanyCategory, bool>> predicate, bool asNoTracking = false, Func<IQueryable<CompanyCategory>, IQueryable<CompanyCategory>>? include = null, CancellationToken ct = default)
            => getItemByPredicate.GetItemByPredicateAsync(predicate, asNoTracking, include, ct);

        public Task<List<CompanyCategory>> GetItemsByPredicateAsync(Expression<Func<CompanyCategory, bool>>? predicate = null, int skip = 0, int? take = null, bool asNoTracking = false, Func<IQueryable<CompanyCategory>, IQueryable<CompanyCategory>>? include = null, CancellationToken ct = default)
            => getItemByPredicate.GetItemsByPredicateAsync(predicate, skip, take, asNoTracking, include, ct);

        public void Create(CompanyCategory item)
            => create.Create(item);

        public void CreateRange(IEnumerable<CompanyCategory> entities)
            => create.CreateRange(entities);
    }
}