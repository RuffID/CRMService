using Microsoft.EntityFrameworkCore;
using CRMService.DataBase;
using CRMService.Models.Entity;
using CRMService.Interfaces.Repository.Entity;

namespace CRMService.Repository.Entity
{
    public class IssueTypeRepository(CRMEntitiesContext context, ILoggerFactory logger) : IIssueTypeRepository
    {
        private readonly ILogger<IssueTypeRepository> _logger = logger.CreateLogger<IssueTypeRepository>();

        public async Task<IEnumerable<IssueType>?> GetItems(int startIndex, int limit)
        {
            try
            {
                return await context.IssueTypes.AsNoTracking().OrderBy(t => t.Id).Where(c => c.Id >= startIndex).Take(limit).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving issue type list.");
                return null;
            }
        }

        public async Task<IssueType?> GetItem(IssueType item, bool? trackable = null)
        {
            try
            {
                if (trackable == null || trackable == true)
                    return await context.IssueTypes.FirstOrDefaultAsync(t => t.Code == item.Code);

                return await context.IssueTypes.AsNoTracking().FirstOrDefaultAsync(t => t.Code == item.Code);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving issue type.");
                return null;
            }
        }

        public async Task<int> GetCountOfItems()
        {
            try
            {
                return await context.IssueTypes.CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving count of issue types.");
                return 0;
            }
        }

        public void Update(IssueType oldItem, IssueType newItem)
        {
            oldItem.CopyData(newItem);
        }

        public void Create(IssueType item)
        {
            context.IssueTypes.Add(item);
        }

        public async Task CreateOrUpdate(IEnumerable<IssueType> items)
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
