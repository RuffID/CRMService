using CRMService.DataBase;
using CRMService.Interfaces.Repository.Server;
using CRMService.Models.Server;
using Microsoft.EntityFrameworkCore;

namespace CRMService.Repository.Server
{
    public class ClientInfoRepository(CRMServerInfoContext context, ILoggerFactory logger) : IClientInfoRepository
    {
        private readonly ILogger<ClientInfoRepository> _logger = logger.CreateLogger<ClientInfoRepository>();

        public async Task<IEnumerable<ClientAppInfo>?> GetItems(int startIndex, int limit)
        {
            try
            {
                return await context.Clients.AsNoTracking().OrderBy(c => c.Id).Where(c => c.Id >= startIndex).Take(limit).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving client app version list.");
                return null;
            }
        }

        public async Task<ClientAppInfo?> GetItem(ClientAppInfo item, bool? trackable = null)
        {
            try
            {
                if (trackable == null || trackable == true)
                    return await context.Clients.FirstOrDefaultAsync(c => c.Version == item.Version);

                return await context.Clients.AsNoTracking().FirstOrDefaultAsync(c => c.Version == item.Version);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving client app version.");
                return null;
            }
        }        

        public async Task<ClientAppInfo?> GetLatestReleaseInfo()
        {
            try
            {
                return await context.Clients.AsNoTracking().OrderByDescending(v => v.ReleaseDate).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving release version.");
                return null;
            }
        }

        public void Create(ClientAppInfo item)
        {
            context.Clients.Add(item);
        }

        public void Delete(ClientAppInfo item)
        {
            context.Clients.Remove(item);
        }
    }
}
