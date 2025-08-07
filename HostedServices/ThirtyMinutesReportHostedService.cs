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
            await Task.Delay(TimeSpan.FromMinutes(timeout), stoppingToken); // Задержка при запуске сервиса 

            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = scopeFactory.CreateScope();
                IssueService issueService = scope.ServiceProvider.GetRequiredService<IssueService>();
                IUnitOfWorkEntities unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWorkEntities>();
                TimeEntryService timeEntryService = scope.ServiceProvider.GetRequiredService<TimeEntryService>();
                EntitySyncService sync = scope.ServiceProvider.GetRequiredService<EntitySyncService>();

                DateTime dateTo = DateTime.Now;
                DateTime dateFrom = dateTo.AddMinutes(-timeout);

                // Обновление заявок через API за определённый промежуток
                await sync.RunExclusive(async () =>
                {
                    await issueService.UpdateIssuesFromCloudApi(dateFrom, dateTo, startIndex: 0, limit: okdeskSettings.Value.LimitForRetrievingEntitiesFromApi, nameof(ThirtyMinutesReportHostedService));

                    // Получение из БД заявок, которые были обновлены за определённый промежуток времени
                    List<Issue>? issuesFromLocalDb = (await unitOfWork.Issue.GetIssuesBetweenUpdateDates(dateFrom, dateTo, startIndex: 0))?.ToList();

                    // Обновление списанного времени по каждой заявке, которая была обновлена в течении определённого промежутка времени
                    if (issuesFromLocalDb != null && issuesFromLocalDb.Count > 0)
                    {
                        foreach (Issue issue in issuesFromLocalDb)
                        {
                            await timeEntryService.UpdateTimeEntriesFromCloudApi(issue.Id);
                        }
                    }
                });

                await Task.Delay(TimeSpan.FromMinutes(timeout), stoppingToken);
            }
        }
    }
}