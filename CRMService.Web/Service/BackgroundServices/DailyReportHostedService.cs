using CRMService.Application.Abstractions.Database.Repository;
using CRMService.Domain.Models.Constants;
using CRMService.Domain.Models.OkdeskEntity;
using CRMService.Application.Service.OkdeskEntity;
using CRMService.Application.Service.Sync;

namespace CRMService.Web.Service.BackgroundServices
{
    public class DailyReportHostedService(IServiceScopeFactory scopeFactory, ILogger<DailyReportHostedService> logger) : BackgroundService
    {
        const int REPORT_TIMEOUT_HOURS = 24;
        const int REPORT_WINDOW_DAYS = 2;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Delay(TimeSpan.FromHours(REPORT_TIMEOUT_HOURS), stoppingToken);

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
                    DateTime dateFrom = dateTo.AddDays(-REPORT_WINDOW_DAYS);

                    await sync.RunExclusive(async () =>
                    {
                        await issueService.UpdateIssuesFromCloudApi(dateFrom, dateTo, startIndex: 0, limit: LimitConstants.LIMIT_FOR_RETRIEVING_ENTITIES_FROM_API, nameof(DailyReportHostedService));

                        List<Issue> issuesFromLocalDb = await unitOfWork.Issue.GetItemsByPredicateAsync(predicate:
                            i => i.DeletedAt == null
                            && i.Id >= 0
                            && i.EmployeesUpdatedAt >= dateFrom
                            && i.EmployeesUpdatedAt <= dateTo,
                            asNoTracking: true, ct: stoppingToken);

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
                    logger.LogError(ex, "[HostedService] Unhandled exception in {ClassName} loop", nameof(DailyReportHostedService));
                }

                await Task.Delay(TimeSpan.FromMinutes(REPORT_TIMEOUT_HOURS), stoppingToken);
            }
        }
    }
}





