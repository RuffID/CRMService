using Microsoft.EntityFrameworkCore;
using CRMService.DataBase;
using CRMService.Models.Entity;
using CRMService.Interfaces.Repository.Entity;

namespace CRMService.Repository.Entity
{
    public class IssueStatusRepository(CRMEntitiesContext context, ILoggerFactory logger) : IIssueStatusRepository
    {
        private readonly ILogger<IssueStatusRepository> _logger = logger.CreateLogger<IssueStatusRepository>();

        public async Task<IEnumerable<IssueStatus>?> GetItems(int startIndex, int limit)
        {
            try
            {
                return await context.IssueStatuses.AsNoTracking().OrderBy(c => c.Id).Where(c => c.Id >= startIndex).Take(limit).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving issue status list.");
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
                _logger.LogError(ex, "Error retrieving issue status.");
                return null;
            }
        }

        public async Task<int> GetCountOfItems()
        {
            try
            {
                return await context.IssueStatuses.CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving count of issue statuses.");
                return 0;
            }
        }

        public void Update(IssueStatus item)
        {
            context.Entry(item).State = EntityState.Modified;
        }

        public void Create(IssueStatus item)
        {
            context.IssueStatuses.Add(item);
        }

        public async Task CreateOrUpdate(IEnumerable<IssueStatus> items)
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
    }
}
