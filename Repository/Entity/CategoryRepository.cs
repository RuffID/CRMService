using CRMService.DataBase;
using CRMService.Interfaces.Repository.Entity;
using CRMService.Models.Entity;
using Microsoft.EntityFrameworkCore;

namespace CRMService.Repository.Entity
{
    public class CategoryRepository(CRMEntitiesContext context, ILoggerFactory logger) : ICompanyCategoryRepository
    {
        private readonly ILogger<CategoryRepository> _logger = logger.CreateLogger<CategoryRepository>();

        public async Task<IEnumerable<CompanyCategory>?> GetItems(int startIndex, int limit)
        {
            try
            {
                return await context.CompanyCategories.AsNoTracking().OrderBy(c => c.Id).Where(c => c.Id >= startIndex).Take(limit).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving company category list.");
                return null;
            }
        }

        public async Task<CompanyCategory?> GetItem(CompanyCategory item, bool? trackable = null)
        {
            try
            {
                if (trackable == null || trackable == true)
                    return await context.CompanyCategories.FirstOrDefaultAsync(c => c.Id == item.Id || c.Code == item.Code);

                return await context.CompanyCategories.AsNoTracking().FirstOrDefaultAsync(c => c.Id == item.Id || c.Code == item.Code);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving company category.");
                return null;
            }
        }

        public async Task<int> GetCountOfItems()
        {
            try
            {
                return await context.CompanyCategories.CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving count of company categories.");
                return 0;
            }
        }

        public void Update(CompanyCategory item)
        {
            context.Entry(item).State = EntityState.Modified;
        }

        public void Create(CompanyCategory item)
        {
            context.CompanyCategories.Add(item);
        }

        public async Task CreateOrUpdate(IEnumerable<CompanyCategory> items)
        {
            foreach (var item in items)
            {
                if (await GetItem(item, false) == null)
                    Create(item);
                else
                    Update(item);
            }
        }
    }
}
