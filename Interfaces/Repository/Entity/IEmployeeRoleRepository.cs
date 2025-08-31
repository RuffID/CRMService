using CRMService.Interfaces.Repository.Base;
using CRMService.Interfaces.Repository.Extended;
using CRMService.Models.Entity;

namespace CRMService.Interfaces.Repository.Entity
{
    public interface IEmployeeRoleRepository : IUpsertByPredicateRepository<EmployeeRole>, ICreateItemRepository<EmployeeRole>, IDeleteItemRepository<EmployeeRole>
    {
        Task<List<EmployeeRole>> GetItems(int skip = 0, int? take = null, bool asNoTracking = false, CancellationToken ct = default);
        Task<EmployeeRole?> GetItem(int employeeId, int roleId, bool asNoTracking = false, CancellationToken ct = default);
        Task<IEnumerable<EmployeeRole>?> GetConnectionsByEmployee(int employeeId, bool asNoTracking = false, CancellationToken ct = default);
    }
}