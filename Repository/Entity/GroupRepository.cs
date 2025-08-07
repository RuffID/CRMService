using Microsoft.EntityFrameworkCore;
using CRMService.DataBase;
using CRMService.Models.Entity;
using CRMService.Interfaces.Repository.Entity;

namespace CRMService.Repository.Entity
{
    public class GroupRepository(CrmEntitiesContext context, ILoggerFactory logger) : IGroupRepository
    {
        private readonly ILogger<GroupRepository> _logger = logger.CreateLogger<GroupRepository>();

        public async Task<IEnumerable<Group>?> GetItems(int startIndex, int limit)
        {
            try
            {
                return await context.Groups.AsNoTracking().OrderBy(c => c.Id).Where(c => c.Id >= startIndex).Take(limit).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving group list.");
                return null;
            }
        }

        public async Task<Group?> GetItem(Group item, bool? trackable = null)
        {
            try
            {
                if (trackable == null || trackable == true)
                    return await context.Groups.FirstOrDefaultAsync(c => c.Id == item.Id);

                return await context.Groups.AsNoTracking().FirstOrDefaultAsync(c => c.Id == item.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving group.");
                return null;
            }
        }

        public void Update(Group oldItem, Group newItem)
        {
            oldItem.CopyData(newItem);
        }

        public void Create(Group item)
        {
            context.Groups.Add(item);
        }

        public async Task CreateOrUpdate(IEnumerable<Group> items)
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
