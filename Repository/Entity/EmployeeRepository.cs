using CRMService.Interfaces.Repository.Base;
using CRMService.Interfaces.Repository.Entity;
using CRMService.Models.Entity;
using System.Linq.Expressions;

namespace CRMService.Repository.Entity
{
    public class EmployeeRepository(IGetItemByIdRepository<Employee, int> getItemByid,
        ICreateItemRepository<Employee> create,
        IUpsertItemByIdRepository<Employee, int> upsertItemById) : IEmployeeRepository
    {
        public Task<Employee?> GetItemById(int id, bool asNoTracking = false, CancellationToken ct = default, params Expression<Func<Employee, object>>[] includes)
            => getItemByid.GetItemById(id, asNoTracking, ct, includes);

        public Task<List<Employee>> GetItemsByPredicateAndSortById(Expression<Func<Employee, bool>>? predicate = null, int skip = 0, int? take = null, bool asNoTracking = false, CancellationToken ct = default, params Expression<Func<Employee, object>>[] includes)
            => getItemByid.GetItemsByPredicateAndSortById(predicate, skip, take, asNoTracking, ct, includes);

        public void Create(Employee item) => create.Create(item);

        public Task Upsert(Employee item, CancellationToken ct = default)
            => upsertItemById.Upsert(item, ct);

        public Task Upsert(IEnumerable<Employee> items, CancellationToken ct = default)
            => upsertItemById.Upsert(items, ct);
    }
}
