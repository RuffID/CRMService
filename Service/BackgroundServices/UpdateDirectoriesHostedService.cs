using CRMService.Service.Sync;

namespace CRMService.Service.BackgroundServices
{
    public class UpdateDirectoriesHostedService(IServiceScopeFactory scopeFactory, ILogger<UpdateDirectoriesHostedService> logger) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(TimeSpan.FromHours(3), stoppingToken);

                    using IServiceScope scope = scopeFactory.CreateScope();

                    Hosted.UpdateDirectoriesService updateDirectoriesService = scope.ServiceProvider.GetRequiredService<Hosted.UpdateDirectoriesService>();

                    EntitySyncService sync = scope.ServiceProvider.GetRequiredService<EntitySyncService>();

                    await sync.RunExclusive(async () => await updateDirectoriesService.RunUpdateDirectories());
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "[HostedService] Unhandled exception in UpdateDirectoriesHostedService loop");
                }
            }
        }
    }
}