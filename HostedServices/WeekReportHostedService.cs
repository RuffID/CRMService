using CRMService.Service.Sync;

namespace CRMService.HostedServices
{
    public class WeekReportHostedService(IServiceScopeFactory scopeFactory) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                // Вычисляет задержку до следующей недели
                await Task.Delay(TimeSpan.FromDays(7), stoppingToken);

                using IServiceScope scope = scopeFactory.CreateScope();

                Service.Hosted.UpdateDirectoriesService updateDirectoriesService = scope.ServiceProvider.GetRequiredService<Service.Hosted.UpdateDirectoriesService>();

                EntitySyncService sync = scope.ServiceProvider.GetRequiredService<EntitySyncService>();

                await sync.RunExclusive(async () => await updateDirectoriesService.RunUpdateDirectories(DateTime.Now.AddDays(-7)));
            }
        }
    }
}