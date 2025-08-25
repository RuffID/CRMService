using Microsoft.EntityFrameworkCore;
using CRMService.DataBase;
using CRMService.Models.Entity;
using CRMService.Interfaces.Repository.Entity;

namespace CRMService.Repository.Entity
{
    public class KindRepository(ApplicationContext context, ILoggerFactory logger) : IKindRepository
    {
        private readonly ILogger<KindRepository> _logger = logger.CreateLogger<KindRepository>();

        public async Task<IEnumerable<Kind>?> GetItems(int startIndex, int limit)
        {
            try
            {
                return await context.Kinds.AsNoTracking().OrderBy(c => c.Id).Skip(startIndex).Where(c => c.Id >= startIndex).Take(limit).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Method:{MethodName}] Error retrieving kind list.", nameof(GetItems));
                return null;
            }
        }

        public async Task<Kind?> GetItem(Kind item, bool? trackable = null)
        {
            try
            {
                if (trackable == null || trackable == true)
                    return await context.Kinds.FirstOrDefaultAsync(c => c.Code == item.Code);

                return await context.Kinds.AsNoTracking().FirstOrDefaultAsync(c => c.Code == item.Code);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Method:{MethodName}] Error retrieving kind.", nameof(GetItem));
                return null;
            }
        }

        public async Task<Kind?> GetKindByCode(string code, bool? trackable = null)
        {
            try
            {
                if (trackable == null || trackable == true)
                    return await context.Kinds.FirstOrDefaultAsync(c => c.Code == code);

                return await context.Kinds.AsNoTracking().FirstOrDefaultAsync(c => c.Code == code);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Method:{MethodName}] Error retrieving kind by code.", nameof(GetKindByCode));
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
                _logger.LogError(ex, "[Method:{MethodName}] Error retrieving count of kinds.", nameof(GetCountOfItems));
                return 0;
            }
        }

        public void Update(Kind oldItem, Kind newItem)
        {
            oldItem.CopyData(newItem);
        }

        public void Create(Kind item)
        {
            context.Kinds.Add(item);
        }

        public async Task CreateOrUpdate(IEnumerable<Kind> items)
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
