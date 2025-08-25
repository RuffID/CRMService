using CRMService.DataBase;
using CRMService.Interfaces.Repository.Entity;
using CRMService.Models.Entity;
using Microsoft.EntityFrameworkCore;

namespace CRMService.Repository.Entity
{
    public class CategoryRepository(ApplicationContext context, ILoggerFactory logger) : ICompanyCategoryRepository
    {
        private readonly ILogger<CategoryRepository> _logger = logger.CreateLogger<CategoryRepository>();

        public async Task<IEnumerable<CompanyCategory>?> GetItems(int startIndex, int limit)
        {
            try
            {
                return await context.CompanyCategories.AsNoTracking().Where(c => c.Id >= startIndex).OrderBy(c => c.Id).Take(limit).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Method:{MethodName}] Error retrieving company category list.", nameof(GetItems));
                return null;
            }
        }

        public async Task<CompanyCategory?> GetItem(CompanyCategory item, bool? trackable = null)
        {
            try
            {
                if (trackable == null || trackable == true)
                    return await context.CompanyCategories.FirstOrDefaultAsync(c => c.Code == item.Code);

                return await context.CompanyCategories.AsNoTracking().FirstOrDefaultAsync(c => c.Code == item.Code);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Method:{MethodName}] Error retrieving company category.", nameof(GetItem));
                return null;
            }
        }

        public async Task<int> GetCountOfItems()
        {
            try
            {
                return await context.CompanyCategories.AsNoTracking().CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Method:{MethodName}] Error retrieving count of company categories.", nameof(GetCountOfItems));
                return 0;
            }
        }

        public void Update(CompanyCategory oldItem, CompanyCategory newItem)
        {
            oldItem.CopyData(newItem);
        }

        public void Create(CompanyCategory item)
        {
            context.CompanyCategories.Add(item);
        }

        public async Task CreateOrUpdate(IEnumerable<CompanyCategory> items)
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
