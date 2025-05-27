using Microsoft.EntityFrameworkCore;
using CRMService.DataBase;
using CRMService.Models.Entity;
using CRMService.Interfaces.Repository.Entity;

namespace CRMService.Repository.Entity
{
    public class ManufacturerRepository(CRMEntitiesContext context, ILoggerFactory logger) : IManufacturerRepository
    {
        private readonly ILogger<ManufacturerRepository> _logger = logger.CreateLogger<ManufacturerRepository>();

        public async Task<IEnumerable<Manufacturer>?> GetItems(int startIndex, int limit)
        {
            try
            {
                return await context.Manufacturers.AsNoTracking().OrderBy(c => c.Id).Where(c => c.Id >= startIndex).Take(limit).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving manufacturer list.");
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
                _logger.LogError(ex, "Error retrieving manufacturer.");
                return null;
            }
        }

        public void Update(Manufacturer item)
        {
            context.Entry(item).State = EntityState.Modified;
        }

        public void Create(Manufacturer item)
        {
            context.Manufacturers.Add(item);
        }

        public async Task CreateOrUpdate(IEnumerable<Manufacturer> items)
        {
            foreach (var item in items)
            {
                var itemFromDb = await GetItem(item);
                if (itemFromDb == null)
                {
                    item.Id = 0;
                    Create(item);
                }
                else
                    itemFromDb.CopyData(item);
            }
        }
    }
}
