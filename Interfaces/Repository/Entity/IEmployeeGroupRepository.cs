using CRMService.Interfaces.Repository.Base;
using CRMService.Models.Entity;

namespace CRMService.Interfaces.Repository.Entity
{
    public interface IEmployeeGroupRepository : IGetItemByPredicateRepository<EmployeeGroup>, IUpsertItemByPredicateRepository<EmployeeGroup>, ICreateItemRepository<EmployeeGroup>, IDeleteItemRepository<EmployeeGroup>
    {
        Task<List<Employee>> GetEmployeesByGroup(int groupId, int employeeStartIndex, bool asNoTracking = false, CancellationToken ct = default);
    }
}