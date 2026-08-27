using CRMService.Application.Abstractions.Database.Repository.Base;
using CRMService.Domain.Models.OkdeskEntity;

namespace CRMService.Application.Abstractions.Database.Repository.Entity
{
    public interface IEmployeeRoleRepository :
        ICreateItemRepository<EmployeeRole>,
        IDeleteItemRepository<EmployeeRole>
    {
        Task<List<EmployeeRole>> GetByEmployeeIdsReadOnlyAsync(IReadOnlyCollection<int> employeeIds, CancellationToken ct = default);
    }
}
