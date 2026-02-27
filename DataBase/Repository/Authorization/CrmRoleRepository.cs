using CRMService.Abstractions.Database.Repository.Authorization;
using EFCoreLibrary.Abstractions.Database.Repository.Base;
using CRMService.Models.Authorization;
using System.Linq.Expressions;

namespace CRMService.DataBase.Repository.Authorization
{
    public class CrmRoleRepository(IGetItemByIdRepository<CrmRole, Guid> getById,
        IGetItemByPredicateRepository<CrmRole> getByPredicate, 
        ICreateItemRepository<CrmRole> create) : ICrmRoleRepository
    {
        public Task<CrmRole?> GetItemByIdAsync(Guid id, bool asNoTracking = false, Func<IQueryable<CrmRole>, IQueryable<CrmRole>>? include = null, CancellationToken ct = default)
            => getById.GetItemByIdAsync(id, asNoTracking, include, ct);

        public Task<CrmRole?> GetItemByPredicateAsync(Expression<Func<CrmRole, bool>> predicate, bool asNoTracking = false, Func<IQueryable<CrmRole>, IQueryable<CrmRole>>? include = null, CancellationToken ct = default)
            => getByPredicate.GetItemByPredicateAsync(predicate, asNoTracking, include, ct);

        public Task<List<CrmRole>> GetItemsByPredicateAsync(Expression<Func<CrmRole, bool>>? predicate = null, int skip = 0, int? take = null, bool asNoTracking = false, Func<IQueryable<CrmRole>, IQueryable<CrmRole>>? include = null, CancellationToken ct = default)
            => getByPredicate.GetItemsByPredicateAsync(predicate, skip, take, asNoTracking, include, ct);

        public void Create(CrmRole item) => create.Create(item);

        public void CreateRange(IEnumerable<CrmRole> entities) => create.CreateRange(entities);
    }
}