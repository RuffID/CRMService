using Microsoft.EntityFrameworkCore;
using CRMService.DataBase;
using CRMService.Models.Entity;
using CRMService.Interfaces.Repository.Entity;

namespace CRMService.Repository.Entity
{
    public class IssueStatusRepository(ApplicationContext context, ILoggerFactory logger) : IIssueStatusRepository
    {
        private readonly ILogger<IssueStatusRepository> _logger = logger.CreateLogger<IssueStatusRepository>();

        public async Task<IEnumerable<IssueStatus>?> GetItems(int startIndex, int limit)
        {
            try
            {
                return await context.IssueStatuses.AsNoTracking().Where(c => c.Id >= startIndex).OrderBy(c => c.Id).Take(limit).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Method:{MethodName}] Error retrieving issue status list.", nameof(GetItems));
                return null;
            }
        }

        public async Task<IssueStatus?> GetItem(IssueStatus item, bool? trackable = null)
        {
            try
            {
                if (trackable == null || trackable == true)
                    return await context.IssueStatuses.FirstOrDefaultAsync(c => c.Code == item.Code);

                return await context.IssueStatuses.AsNoTracking().FirstOrDefaultAsync(c => c.Code == item.Code);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Method:{MethodName}] Error retrieving issue status.", nameof(GetItem));
                return null;
            }
        }

        public async Task<int> GetCountOfItems()
        {
            try
            {
                return await context.IssueStatuses.AsNoTracking().CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Method:{MethodName}] Error retrieving count of issue statuses.", nameof(GetCountOfItems));
                return 0;
            }
        }

        public void Update(IssueStatus oldItem, IssueStatus newItem)
        {
            oldItem.CopyData(newItem);
        }

        public void Create(IssueStatus item)
        {
            context.IssueStatuses.Add(item);
        }

        public async Task CreateOrUpdate(IEnumerable<IssueStatus> items)
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
