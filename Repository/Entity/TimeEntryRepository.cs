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
                _logger.LogError(ex, "[Method:{MethodName}] Error retrieving time entry list. Start index - {startIndex}, limit - {limit}", nameof(GetItems), startIndex, limit);
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
                _logger.LogError(ex, "[Method:{MethodName}] Error retrieving time entry. Id - {entryId}", nameof(GetItem), item.Id);
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
                _logger.LogError(ex, "[Method:{MethodName}] Error retrieving time entries by issue id - {issueId}.", nameof(GetEntriesByIssue), issueId);
                return null;
            }
        }

        public void Create(TimeEntry item)
        {
            context.TimeEntries.Add(item);
        }

        public void Update(TimeEntry oldItem, TimeEntry newItem)
        {
            oldItem.CopyData(newItem);
        }

        public async Task CreateOrUpdate(IEnumerable<TimeEntry> items)
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

        public void Delete(TimeEntry item)
        {
            context.TimeEntries.Remove(item);
        }
    }
}
