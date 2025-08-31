using CRMService.Interfaces.Repository.Base;
using CRMService.Interfaces.Repository.Entity;
using CRMService.Interfaces.Repository.Extended;
using CRMService.Models.Entity;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace CRMService.Repository.Entity
{
    public class OkdeskRoleRepository(IGetItemByIdRepository<OkdeskRole, int> _getByid,
        IQueryRepository<OkdeskRole> _query,
        ICreateItemRepository<OkdeskRole> _create,
        IUpsertItemByIdRepository<OkdeskRole, int> _upsert) : IOkdeskRoleRepository
    {
        public void Create(OkdeskRole item) => _create.Create(item);

        public Task<OkdeskRole?> GetItemById(int id, bool asNoTracking = false, CancellationToken ct = default, params Expression<Func<OkdeskRole, object>>[] includes)
            => _getByid.GetItemById(id, asNoTracking, ct, includes);

        public Task<List<OkdeskRole>> GetItemsByPredicateAndSortById(Expression<Func<OkdeskRole, bool>>? predicate = null, int skip = 0, int? take = null, bool asNoTracking = false, CancellationToken ct = default, params Expression<Func<OkdeskRole, object>>[] includes)
            => _getByid.GetItemsByPredicateAndSortById(predicate, skip, take, asNoTracking, ct, includes);

        public Task<OkdeskRole?> GetRoleByName(string name, bool asNoTracking = false)
            => _query.Query(asNoTracking).FirstOrDefaultAsync(r => r.Name == name);

        public Task Upsert(OkdeskRole item, CancellationToken ct = default)
            => _upsert.Upsert(item, ct);

        public Task Upsert(IEnumerable<OkdeskRole> items, CancellationToken ct = default)
            => _upsert.Upsert(items, ct);
    }
}
