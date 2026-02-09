using CRMService.Service.Sync;

namespace CRMService.Service.BackgroundServices
{
    public class UpdateDirectoriesHostedService(IServiceScopeFactory scopeFactory) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromHours(3), stoppingToken);

                using IServiceScope scope = scopeFactory.CreateScope();

                Hosted.UpdateDirectoriesService updateDirectoriesService = scope.ServiceProvider.GetRequiredService<Hosted.UpdateDirectoriesService>();
             
                EntitySyncService sync = scope.ServiceProvider.GetRequiredService<EntitySyncService>();

                await sync.RunExclusive(async () => await updateDirectoriesService.RunUpdateDirectories());
            }
        }
    }
}