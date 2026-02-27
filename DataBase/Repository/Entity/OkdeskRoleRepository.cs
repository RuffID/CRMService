using EFCoreLibrary.Abstractions.Database.Repository.Base;
using CRMService.Abstractions.Database.Repository.OkdeskEntity;
using CRMService.Models.OkdeskEntity;
using System.Linq.Expressions;

namespace CRMService.DataBase.Repository.Entity
{
    public class OkdeskRoleRepository(IGetItemByIdRepository<OkdeskRole, int> getItemByid,
        IGetItemByPredicateRepository<OkdeskRole> getItemByPredicate,
        ICreateItemRepository<OkdeskRole> create) : IOkdeskRoleRepository
    {
        public Task<OkdeskRole?> GetItemByIdAsync(int id, bool asNoTracking = false, Func<IQueryable<OkdeskRole>, IQueryable<OkdeskRole>>? include = null, CancellationToken ct = default)
            => getItemByid.GetItemByIdAsync(id, asNoTracking, include, ct);

        public Task<OkdeskRole?> GetItemByPredicateAsync(Expression<Func<OkdeskRole, bool>> predicate, bool asNoTracking = false, Func<IQueryable<OkdeskRole>, IQueryable<OkdeskRole>>? include = null, CancellationToken ct = default)
            => getItemByPredicate.GetItemByPredicateAsync(predicate, asNoTracking, include, ct);

        public Task<List<OkdeskRole>> GetItemsByPredicateAsync(Expression<Func<OkdeskRole, bool>>? predicate = null, int skip = 0, int? take = null, bool asNoTracking = false, Func<IQueryable<OkdeskRole>, IQueryable<OkdeskRole>>? include = null, CancellationToken ct = default)
            => getItemByPredicate.GetItemsByPredicateAsync(predicate, skip, take, asNoTracking, include, ct);

        public void Create(OkdeskRole item) => create.Create(item);

        public void CreateRange(IEnumerable<OkdeskRole> entities) => create.CreateRange(entities);
    }
}
