using CRMService.DataBase;
using CRMService.Interfaces.Repository;
using CRMService.Interfaces.Repository.Authorization;
using CRMService.Models.ConfigClass;
using CRMService.Repository.Authorization;
using Microsoft.Extensions.Options;

namespace CRMService.Repository
{
    public class UnitOfWorkAuthorization(IOptions<DatabaseSettings> databaseSettings, ILoggerFactory logger) : IUnitOfWorkAuthorization
    {
        private bool disposed = false;
        private readonly CrmAuthorizationContext _context = new (databaseSettings);
        

        

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }

        public virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
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
