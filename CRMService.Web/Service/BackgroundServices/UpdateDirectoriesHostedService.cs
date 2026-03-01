using CRMService.Application.Service.Hosted;

namespace CRMService.Web.Service.BackgroundServices
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

                    UpdateDirectoriesService updateDirectoriesService = scope.ServiceProvider.GetRequiredService<UpdateDirectoriesService>();

                    await updateDirectoriesService.RunUpdateDirectories(stoppingToken);
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




