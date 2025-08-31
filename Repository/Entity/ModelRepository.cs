using CRMService.Interfaces.Repository.Base;
using CRMService.Interfaces.Repository.Entity;
using CRMService.Interfaces.Repository.Extended;
using CRMService.Models.Entity;
using System.Linq.Expressions;

namespace CRMService.Repository.Entity
{
    public class ModelRepository(IGetItemByIdRepository<Model, int> _getById,
        IGetItemByCodeRepository<Model> _getByCode,
        ICreateItemRepository<Model> _create,
        IUpsertItemByCodeRepository<Model> _upsert) : IModelRepository
    {
        public Task<Model?> GetItemByCode(string code, bool asNoTracking = false, CancellationToken ct = default, params Expression<Func<Model, object>>[] includes)
            => _getByCode.GetItemByCode(code, asNoTracking, ct, includes);

        public Task<Model?> GetItemById(int id, bool asNoTracking = false, CancellationToken ct = default, params Expression<Func<Model, object>>[] includes)
            => _getById.GetItemById(id, asNoTracking, ct, includes);

        public Task<List<Model>> GetItemsByPredicateAndSortById(Expression<Func<Model, bool>>? predicate = null, int skip = 0, int? take = null, bool asNoTracking = false, CancellationToken ct = default, params Expression<Func<Model, object>>[] includes)
            => _getById.GetItemsByPredicateAndSortById(predicate, skip, take, asNoTracking, ct, includes);

        public void Create(Model item) => _create.Create(item);

        public Task UpsertByCode(Model item, CancellationToken ct = default)
            => _upsert.UpsertByCode(item, ct);

        public Task UpsertByCode(string oldCode, Model item, CancellationToken ct = default)
            => _upsert.UpsertByCode(oldCode, item, ct);

        public Task UpsertByCodePairs(IEnumerable<(string OldCode, Model Item)> items, CancellationToken ct = default)
            => _upsert.UpsertByCodePairs(items, ct);

        public Task UpsertByCodes(IEnumerable<Model> items, CancellationToken ct = default)
            => _upsert.UpsertByCodes(items, ct);
    }
}
