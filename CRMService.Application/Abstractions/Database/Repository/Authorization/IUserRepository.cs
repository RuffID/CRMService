using CRMService.Application.Abstractions.Database.Repository.Base;
using CRMService.Domain.Models.Authorization;

namespace CRMService.Application.Abstractions.Database.Repository.Authorization
{
    public interface IUserRepository :
        IGetItemByIdRepository<User, Guid>,
        ICreateItemRepository<User>
    {
        Task<List<User>> GetAllWithRolesAndEmployeeReadOnlyAsync(CancellationToken ct = default);
        Task<User?> GetByLoginWithRolesReadOnlyAsync(string login, bool ignoreCase, CancellationToken ct = default);
        Task<User?> GetByIdWithRolesReadOnlyAsync(Guid id, CancellationToken ct = default);
        Task<User?> GetByIdWithRolesAsync(Guid id, CancellationToken ct = default);
        Task<User?> GetByLoginReadOnlyAsync(string login, Guid? excludedUserId = null, CancellationToken ct = default);
        Task<User?> GetByEmployeeIdReadOnlyAsync(int employeeId, Guid? excludedUserId = null, CancellationToken ct = default);
    }
}
