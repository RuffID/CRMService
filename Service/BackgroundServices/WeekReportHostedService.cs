using CRMService.Service.Hosted;
using CRMService.Service.Sync;

namespace CRMService.Service.BackgroundServices
{
    public class WeekReportHostedService(IServiceScopeFactory scopeFactory, ILogger<WeekReportHostedService> logger) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(TimeSpan.FromDays(7), stoppingToken);

                    using IServiceScope scope = scopeFactory.CreateScope();

                    UpdateDirectoriesService updateDirectoriesService = scope.ServiceProvider.GetRequiredService<UpdateDirectoriesService>();

                    EntitySyncService sync = scope.ServiceProvider.GetRequiredService<EntitySyncService>();

                    await sync.RunExclusive(async () => await updateDirectoriesService.RunUpdateDirectories());
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "[HostedService] Unhandled exception in ThirtyMinutesReportHostedService loop");
                }
            }
        }
    }
}