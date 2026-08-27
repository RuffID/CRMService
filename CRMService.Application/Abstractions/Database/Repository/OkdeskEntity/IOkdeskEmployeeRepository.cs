using CRMService.Domain.Models.OkdeskEntity;

namespace CRMService.Application.Abstractions.Database.Repository.OkdeskEntity
{
    public interface IOkdeskEmployeeRepository
    {
        Task<List<Employee>> GetEmployeesReadOnlyAsync(CancellationToken ct = default);
        Task<List<Employee>> GetContactsByIdsReadOnlyAsync(IReadOnlyCollection<int> ids, CancellationToken ct = default);
        Task<Employee?> GetContactByIdReadOnlyAsync(int id, CancellationToken ct = default);
    }
}
