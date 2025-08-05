using CRMService.Service.Sync;

namespace CRMService.HostedServices
{
    // Cлужба предназначена для обновления всех справочников каждые три часа
    public class UpdateDirectoriesHostedService(IServiceScopeFactory scopeFactory) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Delay(TimeSpan.FromMinutes(45), stoppingToken); // Задержка при запуске сервиса            

            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = scopeFactory.CreateScope();

                Service.Hosted.UpdateDirectoriesService updateDirectoriesService = scope.ServiceProvider.GetRequiredService<Service.Hosted.UpdateDirectoriesService>();
                EntitySyncService sync = scope.ServiceProvider.GetRequiredService<EntitySyncService>();

                TimeSpan remaining = DateTime.Now.AddHours(3) - DateTime.Now;
                await sync.RunExclusive(updateDirectoriesService.RunUpdateDirectories);
                await Task.Delay(remaining, stoppingToken);
            }
        }
    }
}