using Microsoft.EntityFrameworkCore;
using CRMService.DataBase;
using CRMService.Models.Entity;
using CRMService.Interfaces.Repository.Entity;

namespace CRMService.Repository.Entity
{
    public class KindParamsRepository(CRMEntitiesContext context, ILoggerFactory logger) : IKindParamsRepository
    {
        private readonly ILogger<KindParamsRepository> _logger = logger.CreateLogger<KindParamsRepository>();

        public async Task<IEnumerable<KindParam>?> GetItems(int startIndex, int limit)
        {
            try
            {
                return await context.KindParams.AsNoTracking().OrderBy(c => c.Id).Where(c => c.Id >= startIndex).Take(limit).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving kind params connections.");
                return null;
            }
        }

        public async Task<IEnumerable<KindParam>?> GetConnectionsByKind(int id, bool? trackable = null)
        {
            try
            {
                if (trackable == null || trackable == true)
                    return await context.KindParams.Where(c => c.KindId == id).ToListAsync();

                return await context.KindParams.AsNoTracking().Where(c => c.KindId == id).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving kind params connections by kind id, id: {kindId}.", id);
                return null;
            }
        }

        public async Task<KindParam?> GetItem(KindParam item, bool? trackable = null)
        {
            try
            {
                if (trackable == null || trackable == true)
                    return await context.KindParams.FirstOrDefaultAsync(c => c.Id == item.Id);

                return await context.KindParams.AsNoTracking().FirstOrDefaultAsync(c => c.Id == item.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving kind param connection.");
                return null;
            }
        }

        public async Task<KindParam?> GetConnectionByKindId(int id, bool? trackable = null)
        {
            try
            {
                if (trackable == null || trackable == true)
                    return await context.KindParams.FirstOrDefaultAsync(c => c.KindId == id);

                return await context.KindParams.AsNoTracking().FirstOrDefaultAsync(c => c.KindId == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving kind param connection by kind id, id: {kindId}.", id);
                return null;
            }
        }

        public void Create(KindParam item)
        {
            context.KindParams.Add(item);
        }

        public void Delete(KindParam item)
        {
            context.KindParams.Remove(item);
        }
    }
}
