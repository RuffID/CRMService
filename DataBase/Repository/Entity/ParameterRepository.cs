using CRMService.Models.Entity;
using CRMService.Interfaces.Repository.Entity;
using System.Linq.Expressions;
using CRMService.Interfaces.Repository.Base;

namespace CRMService.DataBase.Repository.Entity
{
    public class ParameterRepository(IGetItemByPredicateRepository<EquipmentParameter> getItemByPredicate,
        ICreateItemRepository<EquipmentParameter> create,
        IUpsertItemByPredicateRepository<EquipmentParameter> upsert) : IParameterRepository
    {
        public Task<EquipmentParameter?> GetItemByPredicate(Expression<Func<EquipmentParameter, bool>> predicate, bool asNoTracking = false, CancellationToken ct = default, params Expression<Func<EquipmentParameter, object>>[] includes)
            => getItemByPredicate.GetItemByPredicate(predicate, asNoTracking, ct, includes);

        public Task<List<EquipmentParameter>> GetItemsByPredicate(Expression<Func<EquipmentParameter, bool>>? predicate = null, int skip = 0, int? take = null, bool asNoTracking = false, CancellationToken ct = default, params Expression<Func<EquipmentParameter, object>>[] includes)
            => getItemByPredicate.GetItemsByPredicate(predicate, skip, take, asNoTracking, ct, includes);

        public void Create(EquipmentParameter item) => create.Create(item);

        public Task Upsert(EquipmentParameter item, Expression<Func<EquipmentParameter, bool>> predicate, CancellationToken ct = default)
            => upsert.Upsert(item, predicate, ct);

        public Task Upsert(IEnumerable<EquipmentParameter> items, Func<EquipmentParameter, Expression<Func<EquipmentParameter, bool>>> predicateFactory, CancellationToken ct = default)
            => upsert.Upsert(items, predicateFactory, ct);
    }
}
