using CRMService.DataBase;
using CRMService.Interfaces.Repository.Entity;
using CRMService.Models.Entity;
using Microsoft.EntityFrameworkCore;

namespace CRMService.Repository.Entity
{
    public class CompanyRepository(CrmEntitiesContext context, ILoggerFactory logger) : ICompanyRepository
    {
        private readonly ILogger<CompanyRepository> _logger = logger.CreateLogger<CompanyRepository>();

        public async Task<IEnumerable<Company>?> GetItems(int startIndex, int limit)
        {
            try
            {
                return await context.Companies.AsNoTracking().Where(c => c.Id >= startIndex).OrderBy(c => c.Id).Take(limit).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Method:{MethodName}] Error retrieving company list.", nameof(GetItems));
                return null;
            }
        }

        public async Task<Company?> GetItem(Company item, bool? trackable = null)
        {
            try
            {
                if (trackable == null || trackable == true)
                    return await context.Companies.FirstOrDefaultAsync(c => c.Id == item.Id);

                return await context.Companies.AsNoTracking().FirstOrDefaultAsync(c => c.Id == item.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Method:{MethodName}] Error retrieving company.", nameof(GetItem));
                return null;
            }
        }

        public async Task<Company?> GetCompanyById(int id, bool? trackable = null)
        {
            try
            {
                if (trackable == null || trackable == true)
                    return await context.Companies.FirstOrDefaultAsync(c => c.Id == id);

                return await context.Companies.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Method:{MethodName}] Error retrieving company by id.", nameof(GetCompanyById));
                return null;
            }
        }

        public async Task<IEnumerable<Company>?> GetCompaniesByCategoryCode(string categoryCode, int startIndexCompany, int limit)
        {
            try
            {
                return await context.Companies
                    .AsNoTracking()
                    .Where(c => c.Category != null && c.Category.Code == categoryCode && c.Id >= startIndexCompany)                    
                    .OrderBy(c => c.Id)
                    .Take(limit)
                    .Select(c => new Company
                    {
                        Id = c.Id,
                        Name = c.Name,
                        AdditionalName = c.AdditionalName,
                        Active = c.Active,
                        CategoryId = c.CategoryId
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Method:{MethodName}] Error retrieving company list by category code.", nameof(GetCompaniesByCategoryCode));
                return null;
            }
        }

        public void Update(Company oldItem, Company newItem)
        {
            oldItem.CopyData(newItem);
        }

        public void Create(Company item)
        {
            context.Companies.Add(item);
        }

        public async Task CreateOrUpdate(IEnumerable<Company> items)
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
