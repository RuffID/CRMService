using CRMService.Models.Entity;
using CRMService.Interfaces.Repository.Entity;
using System.Linq.Expressions;
using CRMService.Interfaces.Repository.Extended;
using CRMService.Interfaces.Repository.Base;

namespace CRMService.Repository.Entity
{
    public class ParameterRepository(IGetItemByPredicateRepository<EquipmentParameter> _getByPredicate,
        ICreateItemRepository<EquipmentParameter> _create,
        IUpsertByPredicateRepository<EquipmentParameter> _upsert) : IParameterRepository
    {
        public Task<List<EquipmentParameter>> GetParameterByEquipmentId(int equipmentId, CancellationToken ct)
            => _getByPredicate.GetItemsByPredicate(predicate: p => p.EquipmentId == equipmentId, asNoTracking: true, ct: ct);

        public Task<EquipmentParameter?> GetParameterByEquipmentAndKindParameterId(int equipmentId, int kindParameterId, CancellationToken ct)
            => _getByPredicate.GetItemByPredicate(predicate: p => p.EquipmentId == equipmentId && p.KindParameterId == kindParameterId, asNoTracking: true, ct: ct);

        public Task<EquipmentParameter?> GetItemByPredicate(Expression<Func<EquipmentParameter, bool>> predicate, bool asNoTracking = false, CancellationToken ct = default, params Expression<Func<EquipmentParameter, object>>[] includes)
            => _getByPredicate.GetItemByPredicate(predicate, asNoTracking, ct, includes);

        public Task<List<EquipmentParameter>> GetItemsByPredicate(Expression<Func<EquipmentParameter, bool>>? predicate = null, int skip = 0, int? take = null, bool asNoTracking = false, CancellationToken ct = default, params Expression<Func<EquipmentParameter, object>>[] includes)
            => _getByPredicate.GetItemsByPredicate(predicate, skip, take, asNoTracking, ct, includes);

        public void Create(EquipmentParameter item) => _create.Create(item);

        public Task Upsert(EquipmentParameter item, Expression<Func<EquipmentParameter, bool>> predicate, CancellationToken ct = default)
            => _upsert.Upsert(item, predicate, ct);

        public Task Upsert(IEnumerable<EquipmentParameter> items, Func<EquipmentParameter, Expression<Func<EquipmentParameter, bool>>> predicateFactory, CancellationToken ct = default)
            => _upsert.Upsert(items, predicateFactory, ct);
    }
}
