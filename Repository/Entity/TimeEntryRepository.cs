using Microsoft.EntityFrameworkCore;
using CRMService.DataBase;
using CRMService.Models.Entity;
using CRMService.Interfaces.Repository.Entity;

namespace CRMService.Repository.Entity
{
    public class TimeEntryRepository(CRMEntitiesContext context, ILoggerFactory logger) : ITimeEntryRepository
    {
        private readonly ILogger<TimeEntryRepository> _logger = logger.CreateLogger<TimeEntryRepository>();

        public async Task<IEnumerable<TimeEntry>?> GetItems(int startIndex, int limit)
        {
            try
            {
                return await context.TimeEntries.AsNoTracking().OrderBy(c => c.Id).Where(c => c.Id >= startIndex).Take(limit).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving time entry list.");
                return null;
            }
        }

        public async Task<TimeEntry?> GetItem(TimeEntry item, bool? trackable = null)
        {
            try
            {
                if (trackable == null || trackable == true)
                    return await context.TimeEntries.FirstOrDefaultAsync(c => c.Id == item.Id);

                return await context.TimeEntries.AsNoTracking().FirstOrDefaultAsync(c => c.Id == item.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving time entry.");
                return null;
            }
        }

        public async Task<IEnumerable<TimeEntry>?> GetEntriesByIssue(int issueId, bool? trackable = null)
        {
            try
            {
                if (trackable == null || trackable == true)
                    return await context.TimeEntries.OrderBy(c => c.Id).Where(c => c.IssueId == issueId).ToListAsync();

                return await context.TimeEntries.AsNoTracking().OrderBy(c => c.Id).Where(c => c.IssueId == issueId).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving time entries by issue id.");
                return null;
            }
        }

        public void Update(TimeEntry item)
        {
            context.Entry(item).State = EntityState.Modified;
        }

        public void Create(TimeEntry item)
        {
            context.TimeEntries.Add(item);
        }

        public async Task CreateOrUpdate(IEnumerable<TimeEntry> items)
        {
            foreach (var item in items)
            {
                var itemFromDb = await GetItem(item);
                if (itemFromDb == null)
                    Create(item);
                else
                    itemFromDb.CopyData(item);
            }
        }

        public void Delete(TimeEntry item)
        {
            context.TimeEntries.Remove(item);
        }
    }
}
