using Microsoft.EntityFrameworkCore;
using CRMService.DataBase;
using CRMService.Models.Entity;
using CRMService.Interfaces.Repository.Entity;

namespace CRMService.Repository.Entity
{
    public class KindParameterRepository(ApplicationContext context, ILoggerFactory logger) : IKindParameterRepository
    {
        private readonly ILogger<KindParameterRepository> _logger = logger.CreateLogger<KindParameterRepository>();

        public async Task<IEnumerable<KindsParameter>?> GetItems(int startIndex, int limit)
        {
            try
            {
                return await context.KindsParameters.AsNoTracking().Where(c => c.Id >= startIndex).OrderBy(c => c.Id).Take(limit).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Method:{MethodName}] Error retrieving kind parameter list.", nameof(GetItems));
                return null;
            }
        }

        public async Task<KindsParameter?> GetItem(KindsParameter item, bool? trackable = null)
        {
            try
            {
                if (trackable == null || trackable == true)
                    return await context.KindsParameters.FirstOrDefaultAsync(c => c.Code == item.Code);

                return await context.KindsParameters.AsNoTracking().FirstOrDefaultAsync(c => c.Code == item.Code);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Method:{MethodName}] Error retrieving kind parameter.", nameof(GetItem));
                return null;
            }
        }

        public async Task<KindsParameter?> GetKindParameterByCode(string code, bool? trackable = null)
        {
            try
            {
                if (trackable == null || trackable == true)
                    return await context.KindsParameters.FirstOrDefaultAsync(c => c.Code == code);

                return await context.KindsParameters.AsNoTracking().FirstOrDefaultAsync(c => c.Code == code);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Method:{MethodName}] Error retrieving kind parameter by code.", nameof(GetKindParameterByCode));
                return null;
            }
        }

        public async Task<int> GetCountOfItems()
        {
            try
            {
                return await context.KindsParameters.AsNoTracking().CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Method:{MethodName}] Error retrieving count of kind parameters.", nameof(GetCountOfItems));
                return 0;
            }
        }

        public void Update(KindsParameter oldItem, KindsParameter newItem)
        {
            oldItem.CopyData(newItem);
        }

        public void Create(KindsParameter item)
        {
            context.KindsParameters.Add(item);
        }

        public async Task CreateOrUpdate(IEnumerable<KindsParameter> items)
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
