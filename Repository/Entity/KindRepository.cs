using CRMService.Models.Entity;
using CRMService.Interfaces.Repository.Entity;
using System.Linq.Expressions;
using CRMService.Interfaces.Repository.Base;
using CRMService.Interfaces.Repository.Extended;

namespace CRMService.Repository.Entity
{
    public class KindRepository(IGetItemByIdRepository<Kind, int> _getById,
        IGetItemByCodeRepository<Kind> _getByCode,
        ICreateItemRepository<Kind> _create,
        IUpsertItemByCodeRepository<Kind> _upsert) : IKindRepository
    {
        public Task<Kind?> GetItemByCode(string code, bool asNoTracking = false, CancellationToken ct = default, params Expression<Func<Kind, object>>[] includes)
            => _getByCode.GetItemByCode(code, asNoTracking, ct, includes);

        public Task<Kind?> GetItemById(int id, bool asNoTracking = false, CancellationToken ct = default, params Expression<Func<Kind, object>>[] includes)
            => _getById.GetItemById(id, asNoTracking, ct, includes);

        public Task<List<Kind>> GetItemsByPredicateAndSortById(Expression<Func<Kind, bool>>? predicate = null, int skip = 0, int? take = null, bool asNoTracking = false, CancellationToken ct = default, params Expression<Func<Kind, object>>[] includes)
            => _getById.GetItemsByPredicateAndSortById(predicate, skip, take, asNoTracking, ct, includes);

        public void Create(Kind item) => _create.Create(item);

        public Task UpsertByCode(Kind item, CancellationToken ct = default)
            => _upsert.UpsertByCode(item, ct);

        public Task UpsertByCode(string oldCode, Kind item, CancellationToken ct = default)
            => _upsert.UpsertByCode(oldCode, item, ct);

        public Task UpsertByCodePairs(IEnumerable<(string OldCode, Kind Item)> items, CancellationToken ct = default)
            => _upsert.UpsertByCodePairs(items, ct);

        public Task UpsertByCodes(IEnumerable<Kind> items, CancellationToken ct = default)
            => _upsert.UpsertByCodes(items, ct);
    }
}
