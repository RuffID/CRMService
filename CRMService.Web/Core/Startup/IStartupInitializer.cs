namespace CRMService.Web.Core.Startup;

public interface IStartupInitializer
{
    Task InitializeAsync(CancellationToken ct = default);
}
