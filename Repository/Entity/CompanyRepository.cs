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
                return await context.Companies.AsNoTracking().OrderBy(c => c.Id).Where(c => c.Id >= startIndex).Take(limit).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving company list.");
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
                _logger.LogError(ex, "Error retrieving company.");
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
                _logger.LogError(ex, "Error retrieving company by id.");
                return null;
            }
        }

        public async Task<IEnumerable<Company>?> GetCompaniesByCategoryCode(string categoryCode, int startIndexCompany, int limit)
        {
            try
            {
                return await (from company in context.Companies
                              join category in context.CompanyCategories on company.CategoryId equals category.Id
                              where category.Code == categoryCode && company.Id >= startIndexCompany
                              orderby company.Id
                              select new Company()
                              {
                                  Id = company.Id,
                                  Name = company.Name,
                                  AdditionalName = company.AdditionalName,
                                  Active = company.Active,
                                  CategoryId = company.CategoryId
                              })
                        .AsNoTracking()
                        .Take(limit)
                        .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving company list by category code.");
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
