using CRMService.Domain.Models.Authorization;
using Microsoft.EntityFrameworkCore;

namespace CRMService.Infrastructure.DataBase.Repository.Authorization;

public partial class BlockReasonRepository
{
    public Task<BlockReason?> GetItemByIdAsync(Guid id, CancellationToken ct = default) => getItemById.GetItemByIdAsync(id, ct: ct);
    public Task<BlockReason?> GetItemByIdReadOnlyAsync(Guid id, CancellationToken ct = default) => getItemById.GetItemByIdAsync(id, true, ct: ct);
    public Task<List<BlockReason>> GetItemsAsync(CancellationToken ct = default) => getItemByPredicate.GetItemsByPredicateAsync(ct: ct);
    public Task<List<BlockReason>> GetItemsReadOnlyAsync(CancellationToken ct = default) => getItemByPredicate.GetItemsByPredicateAsync(asNoTracking: true, ct: ct);
}

public partial class CrmRoleRepository
{
    public Task<CrmRole?> GetItemByIdAsync(Guid id, CancellationToken ct = default) => getById.GetItemByIdAsync(id, ct: ct);
    public Task<CrmRole?> GetItemByIdReadOnlyAsync(Guid id, CancellationToken ct = default) => getById.GetItemByIdAsync(id, true, ct: ct);
    public Task<List<CrmRole>> GetItemsAsync(CancellationToken ct = default) => getByPredicate.GetItemsByPredicateAsync(ct: ct);
    public Task<List<CrmRole>> GetItemsReadOnlyAsync(CancellationToken ct = default) => getByPredicate.GetItemsByPredicateAsync(asNoTracking: true, ct: ct);
    public Task<List<CrmRole>> GetByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken ct = default) => getByPredicate.GetItemsByPredicateAsync(x => ids.Contains(x.Id), ct: ct);
}

public partial class SessionRepository
{
    public Task<Session?> GetItemByIdAsync(Guid id, CancellationToken ct = default) => getItemById.GetItemByIdAsync(id, ct: ct);
    public Task<Session?> GetItemByIdReadOnlyAsync(Guid id, CancellationToken ct = default) => getItemById.GetItemByIdAsync(id, true, ct: ct);
    public Task<Session?> GetByRefreshTokenAsync(string refreshToken, CancellationToken ct = default) => getItemByPredicate.GetItemByPredicateAsync(x => x.RefreshToken == refreshToken, ct: ct);
}

public partial class UserRepository
{
    public Task<User?> GetItemByIdAsync(Guid id, CancellationToken ct = default) => getItemById.GetItemByIdAsync(id, ct: ct);
    public Task<User?> GetItemByIdReadOnlyAsync(Guid id, CancellationToken ct = default) => getItemById.GetItemByIdAsync(id, true, ct: ct);
    public Task<List<User>> GetAllWithRolesAndEmployeeReadOnlyAsync(CancellationToken ct = default) => getItemByPredicate.GetItemsByPredicateAsync(asNoTracking: true, include: q => q.Include(x => x.Roles).Include(x => x.Employee), ct: ct);
    public Task<User?> GetByLoginWithRolesReadOnlyAsync(string login, bool ignoreCase, CancellationToken ct = default) => getItemByPredicate.GetItemByPredicateAsync(x => ignoreCase ? x.Login.ToLower() == login.ToLower() : x.Login == login, true, q => q.Include(x => x.Roles), ct);
    public Task<User?> GetByIdWithRolesReadOnlyAsync(Guid id, CancellationToken ct = default) => getItemById.GetItemByIdAsync(id, true, q => q.Include(x => x.Roles), ct);
    public Task<User?> GetByIdWithRolesAsync(Guid id, CancellationToken ct = default) => getItemById.GetItemByIdAsync(id, false, q => q.Include(x => x.Roles), ct);
    public Task<User?> GetByLoginReadOnlyAsync(string login, Guid? excludedUserId = null, CancellationToken ct = default) => getItemByPredicate.GetItemByPredicateAsync(x => x.Login == login && (!excludedUserId.HasValue || x.Id != excludedUserId.Value), true, ct: ct);
    public Task<User?> GetByEmployeeIdReadOnlyAsync(int employeeId, Guid? excludedUserId = null, CancellationToken ct = default) => getItemByPredicate.GetItemByPredicateAsync(x => x.EmployeeId == employeeId && (!excludedUserId.HasValue || x.Id != excludedUserId.Value), true, ct: ct);
}

public partial class UserRoleRepository
{
    public Task<List<UserRole>> GetItemsAsync(CancellationToken ct = default) => getItemByPredicate.GetItemsByPredicateAsync(ct: ct);
    public Task<List<UserRole>> GetItemsReadOnlyAsync(CancellationToken ct = default) => getItemByPredicate.GetItemsByPredicateAsync(asNoTracking: true, ct: ct);
}
