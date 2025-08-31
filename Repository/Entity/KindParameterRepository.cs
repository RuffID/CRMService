using CRMService.Interfaces.Repository.Base;
using CRMService.Interfaces.Repository.Entity;
using CRMService.Interfaces.Repository.Extended;
using CRMService.Models.Entity;
using System.Linq.Expressions;

namespace CRMService.Repository.Entity
{
    public class KindParameterRepository(IGetItemByIdRepository<KindsParameter, int> _getById,
        IGetItemByCodeRepository<KindsParameter> _getByCode,
        ICreateItemRepository<KindsParameter> _create,
        IUpsertItemByCodeRepository<KindsParameter> _upsert) : IKindParameterRepository
    {
        public Task<KindsParameter?> GetItemByCode(string code, bool asNoTracking = false, CancellationToken ct = default, params Expression<Func<KindsParameter, object>>[] includes)
            => _getByCode.GetItemByCode(code, asNoTracking, ct, includes);

        public Task<KindsParameter?> GetItemById(int id, bool asNoTracking = false, CancellationToken ct = default, params Expression<Func<KindsParameter, object>>[] includes)
            => _getById.GetItemById(id, asNoTracking, ct, includes);

        public Task<List<KindsParameter>> GetItemsByPredicateAndSortById(Expression<Func<KindsParameter, bool>>? predicate = null, int skip = 0, int? take = null, bool asNoTracking = false, CancellationToken ct = default, params Expression<Func<KindsParameter, object>>[] includes)
            => _getById.GetItemsByPredicateAndSortById(predicate, skip, take, asNoTracking, ct, includes);

        public void Create(KindsParameter item) => _create.Create(item);

        public Task UpsertByCode(KindsParameter item, CancellationToken ct = default)
            => _upsert.UpsertByCode(item, ct);

        public Task UpsertByCode(string oldCode, KindsParameter item, CancellationToken ct = default)
            => _upsert.UpsertByCode(oldCode, item, ct);

        public Task UpsertByCodePairs(IEnumerable<(string OldCode, KindsParameter Item)> items, CancellationToken ct = default)
            => _upsert.UpsertByCodePairs(items, ct);

        public Task UpsertByCodes(IEnumerable<KindsParameter> items, CancellationToken ct = default)
            => _upsert.UpsertByCodes(items, ct);
    }
}