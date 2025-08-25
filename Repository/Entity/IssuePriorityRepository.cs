using Microsoft.EntityFrameworkCore;
using CRMService.DataBase;
using CRMService.Models.Entity;
using CRMService.Interfaces.Repository.Entity;

namespace CRMService.Repository.Entity
{
    public class IssuePriorityRepository(ApplicationContext context, ILoggerFactory logger) : IIssuePriorityRepository
    {
        private readonly ILogger<IssuePriorityRepository> _logger = logger.CreateLogger<IssuePriorityRepository>();

        public async Task<IEnumerable<IssuePriority>?> GetItems(int startIndex, int limit)
        {
            try
            {
                return await context.IssuePriorities.AsNoTracking().Where(c => c.Id >= startIndex).OrderBy(p => p.Id).Take(limit).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Method:{MethodName}] Error retrieving issue priority list.", nameof(GetItems));
                return null;
            }
        }

        public async Task<IssuePriority?> GetItem(IssuePriority item, bool? trackable = null)
        {
            try
            {
                if (trackable == null || trackable == true)
                    return await context.IssuePriorities.FirstOrDefaultAsync(p => p.Code == item.Code);

                return await context.IssuePriorities.AsNoTracking().FirstOrDefaultAsync(p => p.Code == item.Code);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Method:{MethodName}] Error retrieving issue priority.", nameof(GetItem));
                return null;
            }
        }

        public async Task<int> GetCountOfItems()
        {
            try
            {
                return await context.IssuePriorities.AsNoTracking().CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Method:{MethodName}] Error retrieving count of issue priorities.", nameof(GetCountOfItems));
                return 0;
            }
        }

        public void Update(IssuePriority oldItem, IssuePriority newItem)
        {
            oldItem.CopyData(newItem);
        }

        public void Create(IssuePriority item)
        {
            context.IssuePriorities.Add(item);
        }

        public async Task CreateOrUpdate(IEnumerable<IssuePriority> items)
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
