using CRMService.DataBase;
using CRMService.Interfaces.Repository.Authorization;
using CRMService.Models.Authorization;
using Microsoft.EntityFrameworkCore;

namespace CRMService.Repository.Authorization
{
    public class UserRoleRepository(CrmAuthorizationContext context, ILoggerFactory logger) : IUserRoleRepository
    {
        private readonly ILogger _logger = logger.CreateLogger<UserRoleRepository>();
        private readonly CrmAuthorizationContext _context = context;

        public async Task<IEnumerable<UserRole>?> GetAllItem(Range range)
        {
            try
            {
                return await _context.UserRoles.AsNoTracking().Skip(range.Start.Value).Take(range.End.Value - range.Start.Value).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Method:{MethodName}] Error retrieving user role list.", nameof(GetAllItem));
                return null;
            }
        }

        public async Task<IEnumerable<Role>?> GetRolesByUserId(Guid userId)
        {
            try
            {
                return await _context.UserRoles
                    .Where(ur => ur.UserId == userId && ur.Role != null)
                    .Select(ur => new Role
                    {
                        Id = ur.Role.Id,
                        Name = ur.Role.Name
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Method:{MethodName}] Error retrieving role list by user id.", nameof(GetRolesByUserId));
                return null;
            }
        }

        public async Task<UserRole?> GetItem(UserRole item, bool? trackable = null)
        {
            try
            {
                if (trackable == null || trackable == true)
                    return await _context.UserRoles.FirstOrDefaultAsync(ur => ur.Id == item.Id || ur.UserId == item.UserId || ur.RoleId == item.RoleId);

                return await _context.UserRoles.AsNoTracking().FirstOrDefaultAsync(ur => ur.Id == item.Id || ur.UserId == item.UserId || ur.RoleId == item.RoleId);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Method:{MethodName}] Error retrieving user role.", nameof(GetItem));
                return null;
            }
        }

        public async Task<UserRole?> GetConnectionByUserAndRoleId(UserRole item, bool? trackable = null)
        {
            try
            {
                if (trackable == null || trackable == true)
                    return await _context.UserRoles.FirstOrDefaultAsync(ur => ur.UserId == item.UserId && ur.RoleId == item.RoleId);

                return await _context.UserRoles.AsNoTracking().FirstOrDefaultAsync(ur => ur.UserId == item.UserId && ur.RoleId == item.RoleId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Method:{MethodName}] Error retrieving user role connection.", nameof(GetConnectionByUserAndRoleId));
                return null;
            }
        }

        public void Update(UserRole oldItem, UserRole newItem)
        {
            oldItem.CopyData(newItem);
        }

        public void Create(UserRole item)
        {            
            _context.UserRoles.Add(item);
        }

        public void Delete(UserRole item)
        {
            _context.Remove(item);
        }        
    }
}
