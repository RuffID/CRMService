using CRMService.DataBase;
using CRMService.Interfaces.Repository.Authorization;
using CRMService.Models.Authorization;
using Microsoft.EntityFrameworkCore;

namespace CRMService.Repository.Authorization
{
    public class RoleRepository(CrmAuthorizationContext context, ILoggerFactory logger) : IRoleRepository
    {
        private readonly CrmAuthorizationContext _context = context;
        private readonly ILogger<RoleRepository> _logger = logger.CreateLogger<RoleRepository>();

        public async Task<IEnumerable<Role>?> GetAllItem(Range range)
        {
            try
            {
                return await _context.Roles.OrderBy(r => r.Name).Skip(range.Start.Value).Take(range.End.Value - range.Start.Value).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Method:{MethodName}] Error retrieving role list.", nameof(GetAllItem));
                return null;
            }
        }

        public async Task<Role?> GetItem(Role item, bool? trackable = null)
        {
            try
            {
                if (trackable == null || trackable == true)
                    return await _context.Roles.FirstOrDefaultAsync(r => r.Id == item.Id || r.Name == item.Name);

                return await _context.Roles.AsNoTracking().FirstOrDefaultAsync(r => r.Id == item.Id || r.Name == item.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Method:{MethodName}] Error retrieving role.", nameof(GetItem));
                return null;
            }
        }

        public void Create(Role item)
        {
            _context.Add(item);
        }
    }
}
