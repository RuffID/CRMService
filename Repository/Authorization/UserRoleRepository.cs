using CRMService.Interfaces.Repository.Auth;
using LoginService.DataBase;
using LoginService.Model.Entity;
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
                return await _context.UserRoles.OrderBy(ur => ur.UserId).Skip(range.Start.Value).Take(range.End.Value - range.Start.Value).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user role list.");
                return null;
            }
        }

        public async Task<IEnumerable<Role>?> GetRolesByUserId(Guid userId)
        {
            try
            {
                return await (from role in _context.Roles
                        join userRoles in _context.UserRoles on role.Id equals userRoles.RoleId
                        join user in _context.Users on userRoles.UserId equals user.Id
                        where userRoles.UserId == userId
                        select new Role()
                        {
                            Id = role.Id,
                            Name = role.Name
                        }).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving role list by user id.");
                return null;
            }
        }

        public async Task<UserRole?> GetItem(UserRole item)
        {
            try
            {
                return await _context.UserRoles.FirstOrDefaultAsync(ur => ur.Id == item.Id || ur.UserId == item.UserId || ur.RoleId == item.RoleId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user role.");
                return null;
            }
        }

        public async Task<UserRole?> GetConnectionByUserAndRoleId(UserRole item)
        {
            try
            {
                return await _context.UserRoles.FirstOrDefaultAsync(ur => ur.UserId == item.UserId && ur.RoleId == item.RoleId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user role connection.");
                return null;
            }
        }

        public void Update(UserRole item)
        {
            _context.Entry(item).State = EntityState.Modified;
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
