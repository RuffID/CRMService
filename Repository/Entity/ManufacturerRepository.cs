using CRMService.Models.Entity;
using CRMService.Interfaces.Repository.Entity;
using CRMService.Interfaces.Repository.Base;
using CRMService.Interfaces.Repository.Extended;
using System.Linq.Expressions;

namespace CRMService.Repository.Entity
{
    public class ManufacturerRepository(IGetItemByIdRepository<Manufacturer, int> _getById,
        IGetItemByCodeRepository<Manufacturer> _getByCode,
        ICreateItemRepository<Manufacturer> _create,
        IUpsertItemByCodeRepository<Manufacturer> _upsert) : IManufacturerRepository
    {
        public Task<Manufacturer?> GetItemByCode(string code, bool asNoTracking = false, CancellationToken ct = default, params Expression<Func<Manufacturer, object>>[] includes)
            => _getByCode.GetItemByCode(code, asNoTracking, ct, includes);

        public Task<Manufacturer?> GetItemById(int id, bool asNoTracking = false, CancellationToken ct = default, params Expression<Func<Manufacturer, object>>[] includes)
            => _getById.GetItemById(id, asNoTracking, ct, includes);

        public Task<List<Manufacturer>> GetItemsByPredicateAndSortById(Expression<Func<Manufacturer, bool>>? predicate = null, int skip = 0, int? take = null, bool asNoTracking = false, CancellationToken ct = default, params Expression<Func<Manufacturer, object>>[] includes)
            => _getById.GetItemsByPredicateAndSortById(predicate, skip, take, asNoTracking, ct, includes);

        public void Create(Manufacturer item) => _create.Create(item);

        public Task UpsertByCode(Manufacturer item, CancellationToken ct = default)
            => _upsert.UpsertByCode(item, ct);

        public Task UpsertByCode(string oldCode, Manufacturer item, CancellationToken ct = default)
            => _upsert.UpsertByCode(oldCode, item, ct);

        public Task UpsertByCodePairs(IEnumerable<(string OldCode, Manufacturer Item)> items, CancellationToken ct = default)
            => _upsert.UpsertByCodePairs(items, ct);

        public Task UpsertByCodes(IEnumerable<Manufacturer> items, CancellationToken ct = default)
            => _upsert.UpsertByCodes(items, ct);
    }
}
