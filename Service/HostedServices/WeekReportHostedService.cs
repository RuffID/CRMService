using CRMService.Service.Sync;

namespace CRMService.Service.HostedServices
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

                Hosted.UpdateDirectoriesService updateDirectoriesService = scope.ServiceProvider.GetRequiredService<Hosted.UpdateDirectoriesService>();

                EntitySyncService sync = scope.ServiceProvider.GetRequiredService<EntitySyncService>();

                await sync.RunExclusive(async () => await updateDirectoriesService.RunUpdateDirectories(dateFrom: DateTime.Now.AddDays(-7)));
            }
        }
    }
}