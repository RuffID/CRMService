using CRMService.Service.Sync;

namespace CRMService.HostedServices
{
    public class UpdateDirectoriesHostedService(IServiceScopeFactory scopeFactory) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromHours(3), stoppingToken);

                using IServiceScope scope = scopeFactory.CreateScope();

                Service.Hosted.UpdateDirectoriesService updateDirectoriesService = scope.ServiceProvider.GetRequiredService<Service.Hosted.UpdateDirectoriesService>();
             
                EntitySyncService sync = scope.ServiceProvider.GetRequiredService<EntitySyncService>();

                await sync.RunExclusive(async () => await updateDirectoriesService.RunUpdateDirectories(dateTo: DateTime.Now.AddHours(-3)));
            }
        }
    }
}