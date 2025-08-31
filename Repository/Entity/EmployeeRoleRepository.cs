using Microsoft.EntityFrameworkCore;
using CRMService.Models.Entity;
using CRMService.Interfaces.Repository.Entity;
using CRMService.Interfaces.Repository.Extended;
using CRMService.Interfaces.Repository.Base;
using System.Linq.Expressions;

namespace CRMService.Repository.Entity
{
    public class EmployeeRoleRepository(IQueryRepository<EmployeeRole> _query,
        ICreateItemRepository<EmployeeRole> _create,
        IUpsertByPredicateRepository<EmployeeRole> _upsert,
        IGetItemByPredicateRepository<EmployeeRole> _getByPredicate,
        IDeleteItemRepository<EmployeeRole> _delete) : IEmployeeRoleRepository
    {
        public Task<List<EmployeeRole>> GetItems(int skip = 0, int? take = null, bool asNoTracking = false, CancellationToken ct = default)
            => _getByPredicate.GetItemsByPredicate(skip: skip, take: take, asNoTracking: asNoTracking, ct: ct);

        public Task<EmployeeRole?> GetItem(int employeeId, int roleId, bool asNoTracking = false, CancellationToken ct = default)
            => _getByPredicate.GetItemByPredicate(c => c.EmployeeId == employeeId && c.RoleId == roleId, asNoTracking, ct);

        public async Task<IEnumerable<EmployeeRole>?> GetConnectionsByEmployee(int employeeId, bool asNoTracking = false, CancellationToken ct = default)
            => await _query.Query(asNoTracking).Where(c => c.EmployeeId == employeeId).ToListAsync(ct);

        public void Create(EmployeeRole item) => _create.Create(item);

        public Task Upsert(EmployeeRole item, Expression<Func<EmployeeRole, bool>> predicate, CancellationToken ct = default)
            => _upsert.Upsert(item, predicate, ct);

        public Task Upsert(IEnumerable<EmployeeRole> items, Func<EmployeeRole, Expression<Func<EmployeeRole, bool>>> predicateFactory, CancellationToken ct = default)
            => _upsert.Upsert(items, predicateFactory, ct);

        public void Delete(EmployeeRole item) => _delete.Delete(item);
    }
}
