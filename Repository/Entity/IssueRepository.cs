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
                _logger.LogError(ex, "[Method:{MethodName}] Error retrieving issue list.", nameof(GetItems));
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
                _logger.LogError(ex, "[Method:{MethodName}] Error retrieving issue list between update dates.", nameof(GetIssuesBetweenUpdateDates));
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
                _logger.LogError(ex, "[Method:{MethodName}] Error retrieving issue: {IssueId}.", item.Id, nameof(GetItem));
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
                _logger.LogError(ex, "[Method:{MethodName}] Error retrieving issue by id: {IssueId}.", id, nameof(GetIssueById));
                return null;
            }
        }

        public void Update(Issue oldItem, Issue newItem)
        {
            oldItem.CopyData(newItem);
        }

        public void Create(Issue item)
        {
            context.Issues.Add(item);
        }

        public async Task CreateOrUpdate(IEnumerable<Issue> items)
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

        public async Task CreateOrUpdate(Issue item)
        {
            var existingItem = await GetItem(item);

            if (existingItem == null)
            {
                Create(item);
                return;
            }

            if (item.EmployeesUpdatedAt.HasValue)
            {
                if (!existingItem.EmployeesUpdatedAt.HasValue || item.EmployeesUpdatedAt >= existingItem.EmployeesUpdatedAt)
                {
                    Update(existingItem, item);
                }
            }
        }
    }
}
