using CRMService.Interfaces.Repository.Base;
using CRMService.Interfaces.Repository.Entity;
using CRMService.Interfaces.Repository.Extended;
using CRMService.Models.Entity;
using System.Linq.Expressions;

namespace CRMService.Repository.Entity
{
    public class EmployeeGroupRepository(IGetItemByPredicateRepository<EmployeeGroup> _getByPredicate,
        IUpsertByPredicateRepository<EmployeeGroup> _upsertByPredicate,
        ICreateItemRepository<EmployeeGroup> _create,
        ICountItemRepository<EmployeeGroup> _count,
        IDeleteItemRepository<EmployeeGroup> _delete) : IEmployeeGroupRepository
    {       
        public Task<EmployeeGroup?> GetItem(int employeeId, int groupId, bool asNoTracking = false, CancellationToken ct = default, params Expression<Func<EmployeeGroup, object>>[] includes)
            => _getByPredicate.GetItemByPredicate(eg => eg.EmployeeId == employeeId && eg.GroupId == groupId, asNoTracking, ct, includes);

        public Task<List<EmployeeGroup>> GetItems(int skip = 0, int? take = null, bool asNoTracking = false, CancellationToken ct = default, params Expression<Func<EmployeeGroup, object>>[] includes)
            => _getByPredicate.GetItemsByPredicate(skip: skip, take: take, asNoTracking: asNoTracking, ct: ct, includes: includes);

        public async Task<List<Employee>> GetEmployeesByGroup(int groupId, int startIndex, int limit, bool asNoTracking, CancellationToken ct = default)
        {
            List<EmployeeGroup> groups = await _getByPredicate.GetItemsByPredicate(
                predicate: eg => eg.GroupId == groupId && eg.EmployeeId >= startIndex, asNoTracking: asNoTracking, ct: ct,
                includes: eg => eg.Employee);

            return groups
                .Select(eg => eg.Employee)
                .OrderBy(e => e.Id)
                .Take(limit)
                .ToList();
        }

        public Task<int> GetCountOfItems(Expression<Func<EmployeeGroup, bool>>? predicate = null, CancellationToken ct = default)
            => _count.GetCountOfItems(predicate, ct);

        public Task<List<EmployeeGroup>> GetConnectionsByGroup(int groupId, bool asNoTracking = false, CancellationToken ct = default)
            => _getByPredicate.GetItemsByPredicate(eg => eg.GroupId == groupId, asNoTracking: asNoTracking, ct: ct);

        public Task Upsert(EmployeeGroup item, Expression<Func<EmployeeGroup, bool>> predicate, CancellationToken ct = default)
            => _upsertByPredicate.Upsert(item, predicate, ct);

        public Task Upsert(IEnumerable<EmployeeGroup> items, Func<EmployeeGroup, Expression<Func<EmployeeGroup, bool>>> predicateFactory, CancellationToken ct = default)
            => _upsertByPredicate.Upsert(items, predicateFactory, ct);

        public void Create(EmployeeGroup item) => _create.Create(item);

        public void Delete(EmployeeGroup item) => _delete.Delete(item);        
    }
}