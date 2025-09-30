using CRMService.Interfaces.Repository.Base;
using CRMService.Interfaces.Repository.OkdeskEntity;
using CRMService.Models.OkdeskEntity;
using System.Linq.Expressions;

namespace CRMService.DataBase.Repository.Entity
{
    public class CategoryRepository(IGetItemByIdRepository<CompanyCategory, int> getItemById,
        ICreateItemRepository<CompanyCategory> create,
        IUpsertItemByIdRepository<CompanyCategory, int> upsertItemById,
        IGetItemByPredicateRepository<CompanyCategory> getItemByPredicate) : ICompanyCategoryRepository
    {
        public Task<CompanyCategory?> GetItemById(int id, bool asNoTracking = false, CancellationToken ct = default, params Expression<Func<CompanyCategory, object>>[] includes) 
            => getItemById.GetItemById(id, asNoTracking, ct, includes);

        public Task<List<CompanyCategory>> GetItemsByPredicateAndSortById(Expression<Func<CompanyCategory, bool>>? predicate = null, int skip = 0, int? take = null, bool asNoTracking = false, CancellationToken ct = default, params Expression<Func<CompanyCategory, object>>[] includes) 
            => getItemById.GetItemsByPredicateAndSortById(predicate, skip, take, asNoTracking, ct, includes);

        public Task<CompanyCategory?> GetItemByPredicate(Expression<Func<CompanyCategory, bool>> predicate, bool asNoTracking = false, CancellationToken ct = default, params Expression<Func<CompanyCategory, object>>[] includes)
            => getItemByPredicate.GetItemByPredicate(predicate, asNoTracking, ct, includes);

        public Task<List<CompanyCategory>> GetItemsByPredicate(Expression<Func<CompanyCategory, bool>>? predicate = null, int skip = 0, int? take = null, bool asNoTracking = false, CancellationToken ct = default, params Expression<Func<CompanyCategory, object>>[] includes)
            => getItemByPredicate.GetItemsByPredicate(predicate, skip, take, asNoTracking, ct, includes);

        public Task Upsert(CompanyCategory item, CancellationToken ct = default) 
            => upsertItemById.Upsert(item, ct);

        public void Create(CompanyCategory item) => create.Create(item);

        public Task Upsert(IEnumerable<CompanyCategory> items, CancellationToken ct = default) 
            => upsertItemById.Upsert(items, ct);                
    }
}