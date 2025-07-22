using Microsoft.EntityFrameworkCore;
using CRMService.DataBase;
using CRMService.Models.Entity;
using CRMService.Interfaces.Repository.Entity;

namespace CRMService.Repository.Entity
{
    public class EmployeeRoleRepository(CRMEntitiesContext context, ILoggerFactory logger) : IEmployeeRoleRepository
    {
        private readonly ILogger<EmployeeRoleRepository> _logger = logger.CreateLogger<EmployeeRoleRepository>();

        public async Task<IEnumerable<EmployeeRole>?> GetItems(int startIndex, int limit)
        {
            try
            {
                return await context.EmployeeRoles.AsNoTracking().OrderBy(c => c.Id).Where(c => c.Id >= startIndex).Take(limit).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving employee role list.");
                return null;
            }
        }

        public async Task<EmployeeRole?> GetItem(EmployeeRole item, bool? trackable = null)
        {
            try
            {
                if (trackable == null || trackable == true)
                    return await context.EmployeeRoles.AsNoTracking().FirstOrDefaultAsync(c => c.EmployeeId == item.EmployeeId && c.RoleId == item.RoleId);

                return await context.EmployeeRoles.FirstOrDefaultAsync(c => c.EmployeeId == item.EmployeeId && c.RoleId == item.RoleId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving employee role.");
                return null;
            }
        }

        public async Task<IEnumerable<EmployeeRole>?> GetConnectionsByEmployee(int employeeId)
        {
            try
            {
                return await context.EmployeeRoles.AsNoTracking().OrderBy(c => c.Id).Where(c => c.EmployeeId == employeeId).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving employee role list by employee id.");
                return null;
            }
        }

        public void Update(EmployeeRole oldItem, EmployeeRole newItem)
        {
            oldItem.CopyData(newItem);
        }

        public void Create(EmployeeRole item)
        {
            context.EmployeeRoles.Add(item);
        }

        public async Task CreateOrUpdate(IEnumerable<EmployeeRole> items)
        {
            foreach (var item in items)
            {
                var existingItem = await GetItem(item, false);

                if (existingItem == null)
                    Create(item);
                else
                    Update(existingItem, item);
            }
        }

        public void Delete(EmployeeRole item)
        {
            context.EmployeeRoles.Remove(item);
        }
    }
}
