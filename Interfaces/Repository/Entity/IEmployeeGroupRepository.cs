using CRMService.Interfaces.Repository.Base;
using CRMService.Interfaces.Repository.Extended;
using CRMService.Models.Entity;
using System.Linq.Expressions;

namespace CRMService.Interfaces.Repository.Entity
{
    public interface IEmployeeGroupRepository : IUpsertByPredicateRepository<EmployeeGroup>, ICreateItemRepository<EmployeeGroup>, IDeleteItemRepository<EmployeeGroup>, ICountItemRepository<EmployeeGroup>
    {
        Task<EmployeeGroup?> GetItem(int employeeId, int groupId, bool asNoTracking = false, CancellationToken ct = default, params Expression<Func<EmployeeGroup, object>>[] includes);
        Task<List<EmployeeGroup>> GetItems(int skip = 0, int? take = null, bool asNoTracking = false, CancellationToken ct = default, params Expression<Func<EmployeeGroup, object>>[] includes);
        Task<List<Employee>> GetEmployeesByGroup(int groupId, int startIndex, int limit, bool asNoTracking = false, CancellationToken ct = default);
        Task<List<EmployeeGroup>> GetConnectionsByGroup(int groupId, bool asNoTracking = false, CancellationToken ct = default);
    }
}