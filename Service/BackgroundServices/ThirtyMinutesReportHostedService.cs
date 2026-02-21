using CRMService.Abstractions.Database.Repository;
using CRMService.Models.Constants;
using CRMService.Models.OkdeskEntity;
using CRMService.Service.OkdeskEntity;
using CRMService.Service.Sync;

namespace CRMService.Service.BackgroundServices
{
    public class ThirtyMinutesReportHostedService(IServiceScopeFactory scopeFactory, ILogger<ThirtyMinutesReportHostedService> logger) : BackgroundService
    {
        const int REPORT_TIMEOUT = 30; // задержка в минутах для автоматического запроса
        const int REPORT_WINDOW_MINUTES = 60; // окно в минутах для получения обновлённых заявок с API, должно быть больше, чем REPORT_TIMEOUT, чтобы не пропустить заявки, которые были обновлены между запросами

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Delay(TimeSpan.FromMinutes(REPORT_TIMEOUT), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using IServiceScope scope = scopeFactory.CreateScope();
                    IUnitOfWork unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                    IssueService issueService = scope.ServiceProvider.GetRequiredService<IssueService>();
                    TimeEntryService timeEntryService = scope.ServiceProvider.GetRequiredService<TimeEntryService>();
                    EntitySyncService sync = scope.ServiceProvider.GetRequiredService<EntitySyncService>();

                    DateTime dateTo = DateTime.Now;
                    DateTime dateFrom = dateTo.AddMinutes(-REPORT_WINDOW_MINUTES);

                    // Обновление заявок через API за определённый промежуток
                    await sync.RunExclusive(async () =>
                    {
                        await issueService.UpdateIssuesFromCloudApi(dateFrom, dateTo, startIndex: 0, limit: LimitConstants.LIMIT_FOR_RETRIEVING_ENTITIES_FROM_API, nameof(ThirtyMinutesReportHostedService));

                        // Ниже обновляется списанное время по заявкам, которые были обновлены в течении определённого промежутка времени. Это нужно для того, чтобы в отчетах отображалось актуальное списанное время.
                        // Получение из БД заявок, которые были обновлены за определённый промежуток времени
                        List<Issue> issuesFromLocalDb = await unitOfWork.Issue.GetItemsByPredicateAsync(predicate:
                            i => i.DeletedAt == null
                            && i.Id >= 0
                            && i.EmployeesUpdatedAt >= dateFrom
                            && i.EmployeesUpdatedAt <= dateTo,
                            asNoTracking: true, ct: stoppingToken);

                        // Обновление списанного времени по каждой заявке, которая была обновлена в течении определённого промежутка времени
                        if (issuesFromLocalDb.Count != 0)
                        {
                            foreach (Issue issue in issuesFromLocalDb)
                            {
                                if (issue.DeletedAt.HasValue)
                                    continue;
                                await timeEntryService.UpdateTimeEntriesFromCloudApi(issue.Id, stoppingToken);
                            }
                        }
                    });
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    logger.LogError( ex, "[HostedService] Unhandled exception in {ClassName} loop", nameof(ThirtyMinutesReportHostedService));
                }

                await Task.Delay(TimeSpan.FromMinutes(REPORT_TIMEOUT), stoppingToken);
            }
        }
    }
}