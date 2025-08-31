using CRMService.DataBase;
using CRMService.Interfaces.Repository.Base;
using CRMService.Interfaces.Repository.Entity;
using CRMService.Interfaces.Repository.Extended;
using CRMService.Models.Entity;
using System.Linq.Expressions;

namespace CRMService.Repository.Entity
{
    public class EmployeeRepository(IGetItemByIdRepository<Employee, int> _read,
        ICreateItemRepository<Employee> _create,
        IUpsertItemByIdRepository<Employee, int> _upsertById,
        ICountItemRepository<Employee> _counter) : IEmployeeRepository
    {
        public Task<Employee?> GetItemById(int id, bool asNoTracking = false, CancellationToken ct = default, params Expression<Func<Employee, object>>[] includes)
            => _read.GetItemById(id, asNoTracking, ct, includes);

        public Task<List<Employee>> GetItemsByPredicateAndSortById(Expression<Func<Employee, bool>>? predicate = null, int skip = 0, int? take = null, bool asNoTracking = false, CancellationToken ct = default, params Expression<Func<Employee, object>>[] includes)
            => _read.GetItemsByPredicateAndSortById(predicate, skip, take, asNoTracking, ct, includes);

        public Task<int> GetCountOfItems(Expression<Func<Employee, bool>>? predicate = null, CancellationToken ct = default)
            => _counter.GetCountOfItems(predicate, ct);

        public void Create(Employee item) => _create.Create(item);

        public Task Upsert(Employee item, CancellationToken ct = default)
            => _upsertById.Upsert(item, ct);

        public Task Upsert(IEnumerable<Employee> items, CancellationToken ct = default)
            => _upsertById.Upsert(items, ct);
    }
}
