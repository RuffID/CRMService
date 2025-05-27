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

                await updateDirectoriesService.RunUpdateDirectories();

                TimeSpan remaining = DateTime.Now.AddHours(3) - DateTime.Now;

                await Task.Delay(remaining, stoppingToken);
            }
        }
    }
}