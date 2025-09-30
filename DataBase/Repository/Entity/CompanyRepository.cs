using CRMService.Interfaces.Repository.Base;
using CRMService.Interfaces.Repository.OkdeskEntity;
using CRMService.Models.OkdeskEntity;
using System.Linq.Expressions;

namespace CRMService.DataBase.Repository.Entity
{
    public class CompanyRepository(IGetItemByIdRepository<Company, int> getItemById,
        ICreateItemRepository<Company> create,
        IUpsertItemByIdRepository<Company, int> upsertItemById) : ICompanyRepository
    {
        public Task<List<Company>> GetCompaniesByCategoryCode(string categoryCode, int startIndexCompany, int limit, CancellationToken ct)
            => getItemById.GetItemsByPredicateAndSortById(predicate: c => c.Category != null && c.Category.Code.Equals(categoryCode) && c.Id >= startIndexCompany, asNoTracking: true, ct: ct);

        public Task<Company?> GetItemById(int id, bool asNoTracking = false, CancellationToken ct = default, params Expression<Func<Company, object>>[] includes)
            => getItemById.GetItemById(id, asNoTracking, ct, includes);

        public Task<List<Company>> GetItemsByPredicateAndSortById(Expression<Func<Company, bool>>? predicate = null, int skip = 0, int? take = null, bool asNoTracking = false, CancellationToken ct = default, params Expression<Func<Company, object>>[] includes)
            => getItemById.GetItemsByPredicateAndSortById(predicate, skip, take, asNoTracking, ct, includes);

        public void Create(Company item) => create.Create(item);

        public Task Upsert(Company item, CancellationToken ct = default)
            => upsertItemById.Upsert(item, ct);

        public Task Upsert(IEnumerable<Company> items, CancellationToken ct = default)
            => upsertItemById.Upsert(items, ct);
    }
}
