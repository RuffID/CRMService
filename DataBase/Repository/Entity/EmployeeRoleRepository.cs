using CRMService.Models.OkdeskEntity;
using CRMService.Interfaces.Repository.OkdeskEntity;
using CRMService.Interfaces.Repository.Base;
using System.Linq.Expressions;

namespace CRMService.DataBase.Repository.Entity
{
    public class EmployeeRoleRepository(IGetItemByPredicateRepository<EmployeeRole> getItemByPredicate,
        ICreateItemRepository<EmployeeRole> create,
        IUpsertItemByPredicateRepository<EmployeeRole> upsertItemByPredicate,
        IDeleteItemRepository<EmployeeRole> delete) : IEmployeeRoleRepository
    {
        public Task<EmployeeRole?> GetItemByPredicate(Expression<Func<EmployeeRole, bool>> predicate, bool asNoTracking = false, CancellationToken ct = default, params Expression<Func<EmployeeRole, object>>[] includes)
            => getItemByPredicate.GetItemByPredicate(predicate, asNoTracking, ct, includes);

        public Task<List<EmployeeRole>> GetItemsByPredicate(Expression<Func<EmployeeRole, bool>>? predicate = null, int skip = 0, int? take = null, bool asNoTracking = false, CancellationToken ct = default, params Expression<Func<EmployeeRole, object>>[] includes)
            => getItemByPredicate.GetItemsByPredicate(predicate, skip, take, asNoTracking, ct, includes);

        public void Create(EmployeeRole item) => create.Create(item);

        public Task Upsert(EmployeeRole item, Expression<Func<EmployeeRole, bool>> predicate, CancellationToken ct = default)
            => upsertItemByPredicate.Upsert(item, predicate, ct);

        public Task Upsert(IEnumerable<EmployeeRole> items, Func<EmployeeRole, Expression<Func<EmployeeRole, bool>>> predicateFactory, CancellationToken ct = default)
            => upsertItemByPredicate.Upsert(items, predicateFactory, ct);

        public void Delete(EmployeeRole item) => delete.Delete(item);

        public void DeleteRange(IEnumerable<EmployeeRole> entities) => delete.DeleteRange(entities);
    }
}
