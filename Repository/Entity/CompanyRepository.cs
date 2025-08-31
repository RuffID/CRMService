using CRMService.Interfaces.Repository.Base;
using CRMService.Interfaces.Repository.Entity;
using CRMService.Models.Entity;
using System.Linq.Expressions;

namespace CRMService.Repository.Entity
{
    public class CompanyRepository(IGetItemByIdRepository<Company, int> _read,
        ICreateItemRepository<Company> _create,
        IUpsertItemByIdRepository<Company, int> _upsertById) : ICompanyRepository
    {
        public Task<List<Company>> GetCompaniesByCategoryCode(string categoryCode, int startIndexCompany, int limit, CancellationToken ct)
            => _read.GetItemsByPredicateAndSortById(predicate: c => c.Category != null && c.Category.Code.Equals(categoryCode) && c.Id >= startIndexCompany, asNoTracking: true, ct: ct);

        public Task<Company?> GetItemById(int id, bool asNoTracking = false, CancellationToken ct = default, params Expression<Func<Company, object>>[] includes)
            => _read.GetItemById(id, asNoTracking, ct, includes);

        public Task<List<Company>> GetItemsByPredicateAndSortById(Expression<Func<Company, bool>>? predicate = null, int skip = 0, int? take = null, bool asNoTracking = false, CancellationToken ct = default, params Expression<Func<Company, object>>[] includes)
            => _read.GetItemsByPredicateAndSortById(predicate, skip, take, asNoTracking, ct, includes);

        public void Create(Company item) => _create.Create(item);

        public Task Upsert(Company item, CancellationToken ct = default)
            => _upsertById.Upsert(item, ct);

        public Task Upsert(IEnumerable<Company> items, CancellationToken ct = default)
            => _upsertById.Upsert(items, ct);
    }
}
