using Microsoft.EntityFrameworkCore;
using CRMService.DataBase;
using CRMService.Models.Entity;
using CRMService.Interfaces.Repository.Entity;

namespace CRMService.Repository.Entity
{
    public class IssuePriorityRepository(CRMEntitiesContext context, ILoggerFactory logger) : IIssuePriorityRepository
    {
        private readonly ILogger<IssuePriorityRepository> _logger = logger.CreateLogger<IssuePriorityRepository>();

        public async Task<IEnumerable<IssuePriority>?> GetItems(int startIndex, int limit)
        {
            try
            {
                return await context.IssuePriorities.AsNoTracking().OrderBy(p => p.Id).Where(c => c.Id >= startIndex).Take(limit).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving issue priority list.");
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
                _logger.LogError(ex, "Error retrieving issue priority.");
                return null;
            }
        }

        public async Task<int> GetCountOfItems()
        {
            try
            {
                return await context.IssuePriorities.CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving count of issue priorities.");
                return 0;
            }
        }

        public void Update(IssuePriority item)
        {
            context.Entry(item).State = EntityState.Modified;
        }

        public void Create(IssuePriority item)
        {
            context.IssuePriorities.Add(item);
        }

        public async Task CreateOrUpdate(IEnumerable<IssuePriority> items)
        {
            foreach (var item in items)
            {
                var itemFromDb = await GetItem(item);
                if (itemFromDb == null)
                    Create(item);
                else
                    itemFromDb.CopyData(item);
            }
        }
    }
}
