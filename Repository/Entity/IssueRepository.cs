using Microsoft.EntityFrameworkCore;
using CRMService.DataBase;
using CRMService.Models.Entity;
using CRMService.Interfaces.Repository.Entity;

namespace CRMService.Repository.Entity
{
    public class IssueRepository(CRMEntitiesContext context, ILoggerFactory logger) : IIssueRepository
    {
        private readonly ILogger<IssueRepository> _logger = logger.CreateLogger<IssueRepository>();

        public async Task<IEnumerable<Issue>?> GetItems(int startIndex, int limit)
        {
            try
            {
                return await context.Issues.AsNoTracking().OrderBy(c => c.Id).Where(c => c.Id >= startIndex).Take(limit).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving issue list.");
                return null;
            }
        }

        public async Task<IEnumerable<Issue>?> GetIssuesBetweenUpdateDates(DateTime dateFrom, DateTime dateTo, int startIndex)
        {
            try
            {
                return await context.Issues.AsNoTracking().OrderBy(c => c.Id).Where(c => c.Id >= startIndex && c.EmployeesUpdatedAt > dateFrom && c.EmployeesUpdatedAt < dateTo).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving issue list between update dates.");
                return null;
            }
        }

        public async Task<Issue?> GetItem(Issue item, bool? trackable = null)
        {
            try
            {
                if (trackable == null || trackable == true)
                    return await context.Issues.FirstOrDefaultAsync(c => c.Id == item.Id);

                return await context.Issues.AsNoTracking().FirstOrDefaultAsync(c => c.Id == item.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving issue: {IssueId}.", item.Id);
                return null;
            }
        }

        public async Task<Issue?> GetIssueById(int id, bool? trackable = null)
        {
            try
            {
                if (trackable == null || trackable == true)
                    return await context.Issues.FirstOrDefaultAsync(c => c.Id == id);

                return await context.Issues.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving issue by id: {IssueId}.", id);
                return null;
            }
        }

        public void Update(Issue item)
        {
            context.Entry(item).State = EntityState.Modified;
        }

        public void Create(Issue item)
        {
            context.Issues.Add(item);
        }

        public async Task CreateOrUpdate(IEnumerable<Issue> items)
        {
            foreach (var item in items)
            {
                if (await GetItem(item, false) == null)
                    Create(item);
                else
                    Update(item);
            }
        }

        public async Task CreateOrUpdate(Issue item)
        {
            if (await GetItem(item, false) == null)
                Create(item);
            else
                Update(item);
        }
    }
}
