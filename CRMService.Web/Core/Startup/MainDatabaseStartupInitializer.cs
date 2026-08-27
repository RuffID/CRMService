using CRMService.Infrastructure.DataBase;
using CRMService.Infrastructure.Service.DataBase;

namespace CRMService.Web.Core.Startup;

public class MainDatabaseStartupInitializer(DataBaseCheckUpService<MainContext> databaseCheckUpService)
    : IStartupInitializer
{
    public Task InitializeAsync(CancellationToken ct = default) =>
        databaseCheckUpService.CheckOrUpdateDBAsync(ct);
}
