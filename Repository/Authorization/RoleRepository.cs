using CRMService.DataBase;
using CRMService.Interfaces.Repository.Authorization;
using CRMService.Models.Authorization;
using Microsoft.EntityFrameworkCore;

namespace CRMService.Repository.Authorization
{
    public class RoleRepository(CrmAuthorizationContext context, ILoggerFactory logger) : ICrmRoleRepository
    {
        private readonly CrmAuthorizationContext _context = context;
        private readonly ILogger<RoleRepository> _logger = logger.CreateLogger<RoleRepository>();

        public async Task<IEnumerable<CrmRole>?> GetItems(Range range)
        {
            try
            {
                return await _context.CrmRoles.OrderBy(r => r.Name).Skip(range.Start.Value).Take(range.End.Value - range.Start.Value).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Method:{MethodName}] Error retrieving role list.", nameof(GetItems));
                return null;
            }
        }

        public async Task<ICollection<CrmRole>> GetItems(IEnumerable<CrmRole> items, bool trackable = true)
        {
            try
            {
                if (items == null || !items.Any())
                    return new List<CrmRole>();

                IQueryable<CrmRole> query = trackable
                    ? _context.CrmRoles
                    : _context.CrmRoles.AsNoTracking();

                // Получает список Id и имён из входной коллекции
                List<Guid> ids = items.Where(i => i.Id != Guid.Empty).Select(i => i.Id).ToList();
                List<string> names = items.Where(i => !string.IsNullOrEmpty(i.Name)).Select(i => i.Name!).ToList();

                // Выбирает роли, у которых Id или Name совпадают с переданными
                return await query
                    .Where(r => ids.Contains(r.Id) || names.Contains(r.Name!))
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Method:{MethodName}] Error retrieving role list by collection.", nameof(GetItems));
                return new List<CrmRole>();
            }
        }

        public async Task<CrmRole?> GetItem(CrmRole item, bool? trackable = null)
        {
            try
            {
                if (trackable == null || trackable == true)
                    return await _context.CrmRoles.FirstOrDefaultAsync(r => r.Id == item.Id || r.Name == item.Name);

                return await _context.CrmRoles.AsNoTracking().FirstOrDefaultAsync(r => r.Id == item.Id || r.Name == item.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Method:{MethodName}] Error retrieving role.", nameof(GetItem));
                return null;
            }
        }

        public void Create(CrmRole item)
        {
            _context.Add(item);
        }
    }
}
