using Microsoft.EntityFrameworkCore;
using CRMService.DataBase;
using CRMService.Models.Entity;
using CRMService.Interfaces.Repository.Entity;

namespace CRMService.Repository.Entity
{
    public class ModelRepository(CRMEntitiesContext context, ILoggerFactory logger) : IModelRepository
    {
        private readonly ILogger<ModelRepository> _logger = logger.CreateLogger<ModelRepository>();

        public async Task<IEnumerable<Model>?> GetItems(int startIndex, int limit)
        {
            try
            {
                return await context.Models.AsNoTracking().OrderBy(c => c.Id).Where(c => c.Id >= startIndex).Take(limit).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving model list.");
                return null;
            }
        }

        public async Task<Model?> GetItem(Model item, bool? trackable = null)
        {
            try
            {
                if (trackable == null || trackable == true)
                    return await context.Models.FirstOrDefaultAsync(c => c.Code == item.Code);

                return await context.Models.AsNoTracking().FirstOrDefaultAsync(c => c.Code == item.Code);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving model.");
                return null;
            }
        }

        public void Update(Model oldItem, Model newItem)
        {
            oldItem.CopyData(newItem);
        }

        public void Create(Model item)
        {
            context.Models.Add(item);
        }

        public async Task CreateOrUpdate(IEnumerable<Model> items)
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
