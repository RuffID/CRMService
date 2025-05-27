using CRMService.DataBase;
using CRMService.Interfaces.Repository;
using CRMService.Interfaces.Repository.Server;
using CRMService.Models.ConfigClass;
using CRMService.Repository.Server;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CRMService.Repository
{
    public class UnitOfWorkServerInfo(IOptions<DatabaseSettings> databaseSettings, ILoggerFactory logger) : IUnitOfWorkServerInfo
    {
        private bool disposed = false;
        private readonly CRMServerInfoContext serverInfoContext = new(databaseSettings);
        private readonly ILogger<UnitOfWorkServerInfo> _logger = logger.CreateLogger<UnitOfWorkServerInfo>();
        private IClientInfoRepository? clientInfoRepository;


        public IClientInfoRepository ClientInfo
        {
            get
            {
                clientInfoRepository ??= new ClientInfoRepository(serverInfoContext, logger);
                return (ClientInfoRepository)clientInfoRepository;
            }
        }

        public async Task SaveAsync()
        {
            try
            {
                await serverInfoContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "DbUpdateConcurrencyException error EF.");

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving changes to entity framework.");
            }
        }

        public virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    serverInfoContext.Dispose();
                }
                disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
