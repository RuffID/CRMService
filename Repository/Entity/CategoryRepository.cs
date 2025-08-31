using CRMService.Interfaces.Repository.Base;
using CRMService.Interfaces.Repository.Entity;
using CRMService.Interfaces.Repository.Extended;
using CRMService.Models.Entity;
using System.Linq.Expressions;

namespace CRMService.Repository.Entity
{
    public class CategoryRepository(IGetItemByIdRepository<CompanyCategory, int> _read,
        ICreateItemRepository<CompanyCategory> _create,
        IUpsertItemByIdRepository<CompanyCategory, int> _upsertById,
        IUpsertItemByCodeRepository<CompanyCategory> _upsertByCode,
        IGetItemByCodeRepository<CompanyCategory> _repositoryWithCode,
        ICountItemRepository<CompanyCategory> _count) : ICompanyCategoryRepository
    {
        public Task<CompanyCategory?> GetItemById(int id, bool asNoTracking = false, CancellationToken ct = default, params Expression<Func<CompanyCategory, object>>[] includes) 
            => _read.GetItemById(id, asNoTracking, ct, includes);

        public Task<List<CompanyCategory>> GetItemsByPredicateAndSortById(Expression<Func<CompanyCategory, bool>>? predicate = null, int skip = 0, int? take = null, bool asNoTracking = false, CancellationToken ct = default, params Expression<Func<CompanyCategory, object>>[] includes) 
            => _read.GetItemsByPredicateAndSortById(predicate, skip, take, asNoTracking, ct, includes);

        public Task<CompanyCategory?> GetItemByCode(string code, bool asNoTracking = false, CancellationToken ct = default, params Expression<Func<CompanyCategory, object>>[] includes)
            => _repositoryWithCode.GetItemByCode(code, asNoTracking, ct, includes);

        public Task<int> GetCountOfItems(Expression<Func<CompanyCategory, bool>>? predicate = null, CancellationToken ct = default)
            => _count.GetCountOfItems(predicate, ct);

        public Task Upsert(CompanyCategory item, CancellationToken ct = default) 
            => _upsertById.Upsert(item, ct);

        public void Create(CompanyCategory item) => _create.Create(item);

        public Task Upsert(IEnumerable<CompanyCategory> items, CancellationToken ct = default) 
            => _upsertById.Upsert(items, ct);

        public Task UpsertByCode(CompanyCategory item, CancellationToken ct = default)
            => _upsertByCode.UpsertByCode(item, ct);

        public Task UpsertByCode(string oldCode, CompanyCategory item, CancellationToken ct = default)
            => _upsertByCode.UpsertByCode(oldCode, item, ct);

        public Task UpsertByCodes(IEnumerable<CompanyCategory> items, CancellationToken ct = default)
            => _upsertByCode.UpsertByCodes(items, ct);

        public Task UpsertByCodePairs(IEnumerable<(string OldCode, CompanyCategory Item)> items, CancellationToken ct = default)
            => _upsertByCode.UpsertByCodePairs(items, ct);
    }
}