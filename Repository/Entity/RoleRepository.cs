using Microsoft.EntityFrameworkCore;
using CRMService.DataBase;
using CRMService.Models.Entity;
using CRMService.Interfaces.Repository.Entity;

namespace CRMService.Repository.Entity
{
    public class RoleRepository(CRMEntitiesContext context, ILoggerFactory logger) : IRoleRepository
    {
        private readonly ILogger<RoleRepository> _logger = logger.CreateLogger<RoleRepository>();

        public async Task<IEnumerable<Role>?> GetItems(int startIndex, int limit)
        {
            try
            {
                return await context.Roles.AsNoTracking().OrderBy(c => c.Id).Where(c => c.Id >= startIndex).Take(limit).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving role list.");
                return null;
            }
        }

        public async Task<Role?> GetItem(Role item, bool? trackable = null)
        {
            try
            {
                if (trackable == null || trackable == true)
                    return await context.Roles.FirstOrDefaultAsync(c => c.Id == item.Id);

                return await context.Roles.AsNoTracking().FirstOrDefaultAsync(c => c.Id == item.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving role.");
                return null;
            }
        }

        public async Task<Role?> GetRoleByName(string name, bool? trackable = null)
        {
            try
            {
                if (trackable == null || trackable == true)
                    return await context.Roles.FirstOrDefaultAsync(c => c.Name == name);

                return await context.Roles.AsNoTracking().FirstOrDefaultAsync(c => c.Name == name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving role by name.");
                return null;
            }
        }

        public void Update(Role item)
        {
            context.Entry(item).State = EntityState.Modified;
        }

        public void Create(Role item)
        {
            context.Roles.Add(item);
        }

        public async Task CreateOrUpdate(IEnumerable<Role> items)
        {
            foreach (var item in items)
            {
                if (await GetItem(item, false) == null)
                    Create(item);
                else
                    Update(item);
            }
        }
    }
}
