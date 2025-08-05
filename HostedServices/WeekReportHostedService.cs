using CRMService.Models.ConfigClass;
using CRMService.Service.Entity;
using CRMService.Service.Sync;
using Microsoft.Extensions.Options;

namespace CRMService.HostedServices
{
    // Данная служба предназначена для получения данных семь дней назад на случай если данные были изменены в старых незакрытых задачах
    public class WeekReportHostedService(IOptions<OkdeskSettings> okdeskSettings, IServiceScopeFactory scopeFactory) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Delay(TimeSpan.FromHours(24), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = scopeFactory.CreateScope();
                IssueService issueService = scope.ServiceProvider.GetRequiredService<IssueService>();
                TimeEntryService timeEntryService = scope.ServiceProvider.GetRequiredService<TimeEntryService>();
                EntitySyncService sync = scope.ServiceProvider.GetRequiredService<EntitySyncService>();

                DateTime dateFrom = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, hour: 0, minute: 0, second: 0).AddDays(-7);
                DateTime dateTo = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, hour: 23, minute: 59, second: 59);

                await sync.RunExclusive(async () =>
                {
                    await issueService.UpdateIssuesFromCloudApi(dateFrom, dateTo, startIndex: 0, limit: okdeskSettings.Value.LimitForRetrievingEntitiesFromApi, nameof(WeekReportHostedService));
                    await timeEntryService.UpdateTimeEntriesFromCloudDb(dateFrom, dateTo);
                });

                DateTime nextDay = DateTime.Now.AddDays(7);
                DateTime nextDayTime = new(nextDay.Year, nextDay.Month, nextDay.Day, hour: 0, minute: 0, second: 1);
                TimeSpan remaining = nextDayTime - DateTime.Now;
                await Task.Delay(remaining, stoppingToken);
            }
        }
    }
}