using Microsoft.EntityFrameworkCore;
using CRMService.DataBase;
using CRMService.Models.Entity;
using CRMService.Interfaces.Repository.Entity;

namespace CRMService.Repository.Entity
{
    public class ManufacturerRepository(CrmEntitiesContext context, ILoggerFactory logger) : IManufacturerRepository
    {
        private readonly ILogger<ManufacturerRepository> _logger = logger.CreateLogger<ManufacturerRepository>();

        public async Task<IEnumerable<Manufacturer>?> GetItems(int startIndex, int limit)
        {
            try
            {
                return await context.Manufacturers.AsNoTracking().Where(c => c.Id >= startIndex).OrderBy(c => c.Id).Take(limit).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Method:{MethodName}] Error retrieving manufacturer list.", nameof(GetItems));
                return null;
            }
        }

        public async Task<Manufacturer?> GetItem(Manufacturer item, bool? trackable = null)
        {
            try
            {
                if (trackable == null || trackable == true)
                    return await context.Manufacturers.FirstOrDefaultAsync(c => c.Code == item.Code);

                return await context.Manufacturers.AsNoTracking().FirstOrDefaultAsync(c => c.Code == item.Code);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Method:{MethodName}] Error retrieving manufacturer.", nameof(GetItem));
                return null;
            }
        }

        public void Update(Manufacturer oldItem, Manufacturer newItem)
        {
            oldItem.CopyData(newItem);
        }

        public void Create(Manufacturer item)
        {
            context.Manufacturers.Add(item);
        }

        public async Task CreateOrUpdate(IEnumerable<Manufacturer> items)
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
