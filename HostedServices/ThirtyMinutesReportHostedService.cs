using CRMService.Interfaces.Repository;
using CRMService.Models.ConfigClass;
using CRMService.Models.Entity;
using CRMService.Service.Entity;
using CRMService.Service.Sync;
using Microsoft.Extensions.Options;

namespace CRMService.HostedServices
{
    public class ThirtyMinutesReportHostedService(IOptions<OkdeskSettings> okdeskSettings, IServiceScopeFactory scopeFactory) : BackgroundService
    {
        readonly int timeout = 30; // задержка в минутах для автоматического запроса        

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Delay(TimeSpan.FromMinutes(45), stoppingToken); // Задержка при запуске сервиса 45 минут

            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = scopeFactory.CreateScope();
                IssueService issueService = scope.ServiceProvider.GetRequiredService<IssueService>();
                IUnitOfWorkEntities unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWorkEntities>();
                TimeEntryService timeEntryService = scope.ServiceProvider.GetRequiredService<TimeEntryService>();
                EntitySyncService sync = scope.ServiceProvider.GetRequiredService<EntitySyncService>();

                DateTime now = DateTime.Now;
                DateTime dateTo = new(now.Year, now.Month, now.Day, hour: 23, minute: 59, second: 59);

                // Обновление заявок через API за последние 30 минут
                await sync.RunExclusive(async () =>
                {
                    await issueService.UpdateIssuesFromCloudApi(now, dateTo, startIndex: 0, limit: okdeskSettings.Value.LimitForRetrievingEntitiesFromApi, nameof(ThirtyMinutesReportHostedService));

                    // Получение из БД заявок, которые были обновлены в течении 30 прошедших минут
                    List<Issue>? issuesFromLocalDb = (await unitOfWork.Issue.GetIssuesBetweenUpdateDates(now, dateTo, startIndex: 0))?.ToList();

                    // Обновление списанного времени по каждой заявке, которая была обновлена в течении последних 30 минут
                    if (issuesFromLocalDb != null && issuesFromLocalDb.Count > 0)
                    {
                        foreach (Issue issue in issuesFromLocalDb)
                        {
                            await timeEntryService.UpdateTimeEntriesFromCloudApi(issue.Id);
                        }
                    }
                });

                // Таймаут на 30 минут на следующий парсинг
                TimeSpan remaining = DateTime.UtcNow.AddMinutes(timeout) - DateTime.UtcNow;
                await Task.Delay(remaining, stoppingToken);
            }
        }
    }
}