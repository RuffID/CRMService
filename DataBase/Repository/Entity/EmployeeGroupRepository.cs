using CRMService.Interfaces.Repository.Base;
using CRMService.Interfaces.Repository.OkdeskEntity;
using CRMService.Models.Authorization;
using CRMService.Models.Constants;
using CRMService.Models.OkdeskEntity;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace CRMService.DataBase.Repository.Entity
{
    public class EmployeeGroupRepository(IGetItemByPredicateRepository<EmployeeGroup> getItemByPredicate,
        IQueryRepository<EmployeeGroup> query,
        IUpsertItemByPredicateRepository<EmployeeGroup> upsertItemByPredicate,
        ICreateItemRepository<EmployeeGroup> create,
        IDeleteItemRepository<EmployeeGroup> delete) : IEmployeeGroupRepository
    {
        public Task<EmployeeGroup?> GetItemByPredicate(Expression<Func<EmployeeGroup, bool>> predicate, bool asNoTracking = false, CancellationToken ct = default, params Expression<Func<EmployeeGroup, object>>[] includes)
            => getItemByPredicate.GetItemByPredicate(predicate, asNoTracking, ct, includes);

        public Task<List<EmployeeGroup>> GetItemsByPredicate(Expression<Func<EmployeeGroup, bool>>? predicate = null, int skip = 0, int? take = null, bool asNoTracking = false, CancellationToken ct = default, params Expression<Func<EmployeeGroup, object>>[] includes)
            => getItemByPredicate.GetItemsByPredicate(predicate, skip, take, asNoTracking, ct, includes);

        public async Task<List<Employee>> GetEmployeesByGroup(int groupId, int employeeStartIndex, bool asNoTracking = false, CancellationToken ct = default)
        {
            return await query.Query(asNoTracking)
                .Where(eg => eg.GroupId == groupId && eg.EmployeeId >= employeeStartIndex)
                .OrderBy(eg => eg.EmployeeId)
                .Select(eg => eg.Employee)
                .Take(LimitConstants.LIMIT_FOR_RETRIEVING_ENTITIES_FROM_DB)
                .ToListAsync(ct);
        }

        public Task Upsert(EmployeeGroup item, Expression<Func<EmployeeGroup, bool>> predicate, CancellationToken ct = default)
            => upsertItemByPredicate.Upsert(item, predicate, ct);

        public Task Upsert(IEnumerable<EmployeeGroup> items, Func<EmployeeGroup, Expression<Func<EmployeeGroup, bool>>> predicateFactory, CancellationToken ct = default)
            => upsertItemByPredicate.Upsert(items, predicateFactory, ct);

        public void Create(EmployeeGroup item) => create.Create(item);

        public void Delete(EmployeeGroup item) => delete.Delete(item);

        public void DeleteRange(IEnumerable<EmployeeGroup> entities) => delete.DeleteRange(entities);
    }
}