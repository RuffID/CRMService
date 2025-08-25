using Microsoft.EntityFrameworkCore;
using CRMService.DataBase;
using CRMService.Models.Entity;
using CRMService.Interfaces.Repository.Entity;

namespace CRMService.Repository.Entity
{
    public class MaintenanceEntityRepository(ApplicationContext context, ILoggerFactory logger) : IMaintenanceEntityRepository
    {
        private readonly ILogger<MaintenanceEntityRepository> _logger = logger.CreateLogger<MaintenanceEntityRepository>();

        public async Task<IEnumerable<MaintenanceEntity>?> GetItems(int startIndex, int limit)
        {
            try
            {
                return await context.MaintenanceEntities.AsNoTracking().Where(c => c.Id >= startIndex).OrderBy(c => c.Id).Take(limit).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Method:{MethodName}] Error retrieving maintenance entity list.", nameof(GetItems));
                return null;
            }
        }

        public async Task<MaintenanceEntity?> GetItem(MaintenanceEntity item, bool? trackable = null)
        {
            try
            {
                if (trackable == null || trackable == true)
                    return await context.MaintenanceEntities.FirstOrDefaultAsync(c => c.Id == item.Id);

                return await context.MaintenanceEntities.AsNoTracking().FirstOrDefaultAsync(c => c.Id == item.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Method:{MethodName}] Error retrieving maintenance entity.", nameof(GetItem));
                return null;
            }
        }

        public async Task<MaintenanceEntity?> GetMaintenanceEntityById(int id, bool? trackable = null)
        {
            try
            {
                if (trackable == null || trackable == true)
                    return await context.MaintenanceEntities.FirstOrDefaultAsync(c => c.Id == id);

                return await context.MaintenanceEntities.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Method:{MethodName}] Error retrieving maintenance entity by id.", nameof(GetMaintenanceEntityById));
                return null;
            }
        }

        public void Update(MaintenanceEntity oldItem, MaintenanceEntity newItem)
        {
            oldItem.CopyData(newItem);
        }

        public void Create(MaintenanceEntity item)
        {
            context.MaintenanceEntities.Add(item);
        }

        public async Task CreateOrUpdate(IEnumerable<MaintenanceEntity> items)
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
