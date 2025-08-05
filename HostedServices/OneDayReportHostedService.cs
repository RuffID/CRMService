using CRMService.Service.Sync;

namespace CRMService.HostedServices
{
    public class OneDayReportHostedService(IServiceScopeFactory scopeFactory) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                // Вычисляет задержку до ближайшей полуночи
                DateTime now = DateTime.Now;
                DateTime nextRun = now.Date.AddDays(1); // Завтра в 00:00
                TimeSpan delay = nextRun - now;

                await Task.Delay(delay, stoppingToken); // Ждёт до 00:00

                using IServiceScope scope = scopeFactory.CreateScope();

                Service.Hosted.UpdateDirectoriesService updateDirectoriesService = scope.ServiceProvider.GetRequiredService<Service.Hosted.UpdateDirectoriesService>();
             
                EntitySyncService sync = scope.ServiceProvider.GetRequiredService<EntitySyncService>();

                await sync.RunExclusive(async () => await updateDirectoriesService.RunUpdateDirectories());
            }
        }
    }
}