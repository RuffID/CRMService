using CRMService.Interfaces.Repository.Base;
using CRMService.Interfaces.Repository.OkdeskEntity;
using CRMService.Models.OkdeskEntity;
using System.Linq.Expressions;

namespace CRMService.DataBase.Repository.Entity
{
    public class CompanyRepository(IGetItemByIdRepository<Company, int> getItemById,
        IGetItemByPredicateRepository<Company> getItemByPredicate,
        ICreateItemRepository<Company> create) : ICompanyRepository
    {
        public Task<Company?> GetItemByIdAsync(int id, bool asNoTracking = false, Func<IQueryable<Company>, IQueryable<Company>>? include = null, CancellationToken ct = default)
            => getItemById.GetItemByIdAsync(id, asNoTracking, include, ct);

        public Task<Company?> GetItemByPredicateAsync(Expression<Func<Company, bool>> predicate, bool asNoTracking = false, Func<IQueryable<Company>, IQueryable<Company>>? include = null, CancellationToken ct = default)
            => getItemByPredicate.GetItemByPredicateAsync(predicate, asNoTracking, include, ct);

        public Task<List<Company>> GetItemsByPredicateAsync(Expression<Func<Company, bool>>? predicate = null, int skip = 0, int? take = null, bool asNoTracking = false, Func<IQueryable<Company>, IQueryable<Company>>? include = null, CancellationToken ct = default)
            => getItemByPredicate.GetItemsByPredicateAsync(predicate, skip, take, asNoTracking, include, ct);

        public void Create(Company item) => create.Create(item);

        public void CreateRange(IEnumerable<Company> entities) => create.CreateRange(entities);
    }
}
