using Microsoft.EntityFrameworkCore;
using CRMService.DataBase;
using CRMService.Models.Entity;
using CRMService.Interfaces.Repository.Entity;

namespace CRMService.Repository.Entity
{
    public class EmployeeGroupRepository(CrmEntitiesContext context, ILoggerFactory logger) : IEmployeeGroupRepository
    {
        private readonly ILogger<EmployeeGroupRepository> _logger = logger.CreateLogger<EmployeeGroupRepository>();

        public async Task<IEnumerable<EmployeeGroup>?> GetItems(int startIndex, int limit)
        {
            try
            {
                return await context.EmployeeGroups.AsNoTracking().Where(c => c.Id >= startIndex).OrderBy(c => c.Id).Take(limit).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Method:{MethodName}] Error retrieving employee group list.", nameof(GetItems));
                return null;
            }
        }

        public async Task<EmployeeGroup?> GetItem(EmployeeGroup item, bool? trackable = null)
        {
            try
            {
                if (trackable == null || trackable == true)
                    return await context.EmployeeGroups.FirstOrDefaultAsync(c => c.Id == item.Id);

                return await context.EmployeeGroups.AsNoTracking().FirstOrDefaultAsync(c => c.Id == item.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Method:{MethodName}] Error retrieving employee group.", nameof(GetItem));
                return null;
            }
        }

        public async Task<EmployeeGroup?> GetConnectionByEmployeeAndGroup(int employeeId, int groupId)
        {
            try
            {
                return await context.EmployeeGroups.AsNoTracking().FirstOrDefaultAsync(c => c.EmployeeId == employeeId && c.GroupId == groupId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Method:{MethodName}] Error retrieving connection by employee group and group id.", nameof(GetItem));
                return null;
            }
        }

        public async Task<IEnumerable<EmployeeGroup>?> GetConnectionsByGroup(int groupId)
        {
            try
            {
                return await context.EmployeeGroups.AsNoTracking().Where(c => c.GroupId == groupId).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Method:{MethodName}] Error retrieving connections employee group by employee.", nameof(GetItem));
                return null;
            }
        }

        public async Task<int> GetCountOfItems()
        {
            try
            {
                return await context.EmployeeGroups.AsNoTracking().CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Method:{MethodName}] Error retrieving count of employee groups connection.", nameof(GetItem));
                return 0;
            }
        }

        public void Update(EmployeeGroup oldItem, EmployeeGroup newItem)
        {
            oldItem.CopyData(newItem);
        }

        public void Create(EmployeeGroup item)
        {
            context.EmployeeGroups.Add(item);
        }

        public async Task CreateOrUpdate(IEnumerable<EmployeeGroup> items)
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

        public void Delete(EmployeeGroup item)
        {
            context.EmployeeGroups.Remove(item);
        }
    }
}