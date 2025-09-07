using CRMService.Interfaces.Repository.Authorization;
using CRMService.Interfaces.Repository.Base;
using CRMService.Models.Authorization;
using System.Linq.Expressions;

namespace CRMService.Repository.Authorization
{
    public class CrmRoleRepository(IGetItemByIdRepository<CrmRole, Guid> getById,
        IGetItemByPredicateRepository<CrmRole> getByPredicate, 
        ICreateItemRepository<CrmRole> create) : ICrmRoleRepository
    {
        public Task<CrmRole?> GetItemById(Guid id, bool asNoTracking = false, CancellationToken ct = default, params Expression<Func<CrmRole, object>>[] includes)
            => getById.GetItemById(id, asNoTracking, ct, includes);

        public Task<List<CrmRole>> GetItemsByPredicateAndSortById(Expression<Func<CrmRole, bool>>? predicate = null, int skip = 0, int? take = null, bool asNoTracking = false, CancellationToken ct = default, params Expression<Func<CrmRole, object>>[] includes)
            => getById.GetItemsByPredicateAndSortById(predicate, skip, take, asNoTracking, ct, includes);

        public Task<List<CrmRole>> GetItemsByCollection(IEnumerable<CrmRole> items, bool asNoTracking = false, CancellationToken ct = default)
        {
            if (items == null || !items.Any())
                return Task.FromResult(new List<CrmRole>());

            List<Guid> ids = items.Select(i => i.Id).ToList();
            List<string> names = items.Select(i => i.Name).ToList();

            return getByPredicate.GetItemsByPredicate(predicate: r => ids.Contains(r.Id) || names.Contains(r.Name), asNoTracking: asNoTracking, ct: ct);
        }

        public Task<CrmRole?> GetItemByPredicate(Expression<Func<CrmRole, bool>> predicate, bool asNoTracking = false, CancellationToken ct = default, params Expression<Func<CrmRole, object>>[] includes)
            => getByPredicate.GetItemByPredicate(predicate, asNoTracking, ct, includes);

        public Task<List<CrmRole>> GetItemsByPredicate(Expression<Func<CrmRole, bool>>? predicate = null, int skip = 0, int? take = null, bool asNoTracking = false, CancellationToken ct = default, params Expression<Func<CrmRole, object>>[] includes)
            => getByPredicate.GetItemsByPredicate(predicate, skip, take, asNoTracking, ct, includes);

        public void Create(CrmRole item) => create.Create(item);
    }
}
