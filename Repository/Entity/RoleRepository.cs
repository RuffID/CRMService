using Microsoft.EntityFrameworkCore;
using CRMService.DataBase;
using CRMService.Models.Entity;
using CRMService.Interfaces.Repository.Entity;

namespace CRMService.Repository.Entity
{
    public class RoleRepository(ApplicationContext context, ILoggerFactory logger) : IRoleRepository
    {
        private readonly ILogger<RoleRepository> _logger = logger.CreateLogger<RoleRepository>();

        public async Task<IEnumerable<OkdeskRole>?> GetItems(int startIndex, int limit)
        {
            try
            {
                return await context.OkdeskRoles.AsNoTracking().Where(c => c.Id >= startIndex).OrderBy(c => c.Id).Take(limit).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Method:{MethodName}] Error retrieving role list.", nameof(GetItems));
                return null;
            }
        }

        public async Task<OkdeskRole?> GetItem(OkdeskRole item, bool? trackable = null)
        {
            try
            {
                if (trackable == null || trackable == true)
                    return await context.OkdeskRoles.FirstOrDefaultAsync(c => c.Id == item.Id);

                return await context.OkdeskRoles.AsNoTracking().FirstOrDefaultAsync(c => c.Id == item.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Method:{MethodName}] Error retrieving role.", nameof(GetItem));
                return null;
            }
        }

        public async Task<OkdeskRole?> GetRoleByName(string name, bool? trackable = null)
        {
            try
            {
                if (trackable == null || trackable == true)
                    return await context.OkdeskRoles.FirstOrDefaultAsync(c => c.Name == name);

                return await context.OkdeskRoles.AsNoTracking().FirstOrDefaultAsync(c => c.Name == name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Method:{MethodName}] Error retrieving role by name.", nameof(GetRoleByName));
                return null;
            }
        }

        public void Update(OkdeskRole oldItem, OkdeskRole newItem)
        {
            oldItem.CopyData(newItem);
        }

        public void Create(OkdeskRole item)
        {
            context.OkdeskRoles.Add(item);
        }

        public async Task CreateOrUpdate(IEnumerable<OkdeskRole> items)
        {
            foreach (var item in items)
            {
                var existingItem = await GetItem(item);

                if (existingItem == null)
                    Create(item);
                else
                    Update(existingItem, item);
            }
        }
    }
}
