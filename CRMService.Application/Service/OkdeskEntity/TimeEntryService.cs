using CRMService.Application.Abstractions.Database.Repository;
using CRMService.Application.Common.Exceptions;
using CRMService.Application.Models.ConfigClass;
using CRMService.Domain.Models.Constants;
using CRMService.Domain.Models.OkdeskEntity;
using Microsoft.Extensions.Options;
using CRMService.Application.Abstractions.Service;
using Microsoft.Extensions.Logging;

namespace CRMService.Application.Service.OkdeskEntity
{
    public class TimeEntryService(IOptions<ApiEndpointOptions> endpoint, IOptions<OkdeskOptions> okdesk, IOkdeskEntityRequestService request, IIssuesUnitOfWork unitOfWork, IOkdeskIssuesSource okdeskUnitOfWork, ILogger<TimeEntryService> logger)
    {
        public async Task<TimeEntries?> GetimeEntriesFromCloudApi(int issueId, CancellationToken ct)
        {
            string link = $"{endpoint.Value.OkdeskApi}/issues/{issueId}/time_entries?api_token={okdesk.Value.OkdeskApiToken}";

            return await request.GetItemAsync<TimeEntries>(link, ct);
        }

        public async Task UpdateTimeEntriesFromCloudApi(int issueId, CancellationToken ct)
        {
            TimeEntries? timeEntry;

            try
            {
                timeEntry = await GetimeEntriesFromCloudApi(issueId, ct);
            }
            catch (RemoteResourceNotFoundException)
            {
                logger.LogWarning("[Method:{MethodName}] Issue {IssueId} not found in Okdesk (404). Marking as deleted.", nameof(UpdateTimeEntriesFromCloudApi), issueId);

                Issue? issue = await unitOfWork.Issue.GetItemByIdAsync(issueId, ct: ct);
                if (issue != null && issue.DeletedAt == null)
                {
                    issue.DeletedAt = DateTime.UtcNow;
                    await unitOfWork.SaveChangesAsync(ct);
                }

                List<TimeEntry> existingTimeEntries = await unitOfWork.TimeEntry.GetByIssueIdReadOnlyAsync(issueId, ct);

                if (existingTimeEntries.Count > 0)
                {
                    unitOfWork.TimeEntry.DeleteRange(existingTimeEntries);
                    await unitOfWork.SaveChangesAsync(ct);
                }

                return;
            }

            // Если пустые записи Time_entries, значит не удалось спарсить списанное время/его нет и выходит из метода
            if (timeEntry?.Time_Entries == null || timeEntry.Time_Entries.Length == 0)
            {
                List<TimeEntry> existingTimeEntries = await unitOfWork.TimeEntry.GetByIssueIdReadOnlyAsync(issueId, ct);

                if (existingTimeEntries.Count > 0)
                {
                    unitOfWork.TimeEntry.DeleteRange(existingTimeEntries);
                    await unitOfWork.SaveChangesAsync(ct);
                }

                return;
            }

            foreach (TimeEntry entry in timeEntry.Time_Entries)
            {
                entry.EmployeeId = entry.Employee?.Id 
                    ?? throw new InvalidOperationException($"Employee id is not set in time entry: {entry.Id}");

                entry.IssueId = issueId;

                if (await unitOfWork.Employee.GetItemByIdReadOnlyAsync(entry.EmployeeId, ct) == null)
                {
                    logger.LogWarning("[Method:{MethodName}] Employee {EmployeeId} not found in local DB.", 
                        nameof(UpdateTimeEntriesFromCloudApi), entry.EmployeeId);

                    continue;
                }

                entry.Employee = null;
                entry.Issue = null;

                await CreateOrUpdate(entry, TimeEntryDataSource.Realtime, ct);
            }

            await DeleteMarkedAsDeletedTimeEntries(timeEntry.Time_Entries, ct);
        }


        private async Task<List<TimeEntry>> GetTimeEntriesFromCloudDb(DateTime dateFrom, DateTime dateTo, long startId, long limit, CancellationToken ct)
        {
            List<TimeEntry> timeEntries = await okdeskUnitOfWork.TimeEntry.GetLoggedItemsAsync(dateFrom, dateTo, startId, limit, ct);

            return timeEntries.OrderBy(x => x.Id).ToList();
        }

        public async Task UpdateTimeEntriesFromCloudDb(DateTime dateFrom, DateTime dateTo, CancellationToken ct)
        {
            logger.LogInformation("[Method:{MethodName}] Starting to update time entries from DB.", nameof(UpdateTimeEntriesFromCloudDb));

            long startId = 0;

            while (true)
            {
                List<TimeEntry> entries = await GetTimeEntriesFromCloudDb(dateFrom, dateTo, startId, LimitConstants.LIMIT_FOR_RETRIEVING_ENTITIES_FROM_DB, ct);

                if (entries.Count == 0)
                    break;

                foreach (TimeEntry item in entries)
                    await CreateOrUpdate(item, TimeEntryDataSource.SqlSnapshot, ct);

                startId = entries.Last().Id;

                if (entries.Count < LimitConstants.LIMIT_FOR_RETRIEVING_ENTITIES_FROM_DB)
                    break;
            }

            logger.LogInformation("[Method:{MethodName}] Time entries update completed.", nameof(UpdateTimeEntriesFromCloudDb));
        }

        public Task CreateOrUpdate(TimeEntry entry, CancellationToken ct)
        {
            return CreateOrUpdate(entry, TimeEntryDataSource.Realtime, ct);
        }

        private async Task CreateOrUpdate(TimeEntry entry, TimeEntryDataSource dataSource, CancellationToken ct)
        {
            Issue? existingIssue = await unitOfWork.Issue.GetItemByIdReadOnlyAsync(entry.IssueId, ct);

            if (existingIssue == null)
            {
                logger.LogWarning("[Method:{MethodName}] Issue: {issueId} - not found in local DB.", nameof(CreateOrUpdate), entry.IssueId);
                return;
            }

            TimeEntry? existingEntry = await unitOfWork.TimeEntry.GetItemByIdAsync(entry.Id, ct: ct);

            if (existingEntry == null)
                unitOfWork.TimeEntry.Create(entry);
            else
                Merge(existingEntry, entry, dataSource);

            await unitOfWork.SaveChangesAsync(ct);
        }

        private static void Merge(TimeEntry existingEntry, TimeEntry incomingEntry, TimeEntryDataSource dataSource)
        {
            if (dataSource == TimeEntryDataSource.SqlSnapshot)
            {
                existingEntry.CreatedAt ??= incomingEntry.CreatedAt;
                return;
            }

            existingEntry.EmployeeId = incomingEntry.EmployeeId;
            existingEntry.SpentTime = incomingEntry.SpentTime;
            existingEntry.IssueId = incomingEntry.IssueId;
            existingEntry.LoggedAt = incomingEntry.LoggedAt;

            if (incomingEntry.CreatedAt.HasValue)
                existingEntry.CreatedAt = incomingEntry.CreatedAt;
        }

        private async Task DeleteMarkedAsDeletedTimeEntries(TimeEntry[] entriesFromCloudApi, CancellationToken ct)
        {
            // сгруппировать облачные записи по IssueId
            var groups = entriesFromCloudApi
            .GroupBy(e => e.IssueId)
            .Select(g => new
            {
                IssueId = g.Key,
                CloudIds = g.Select(x => x.Id).Distinct().ToList()
            })
            .ToList();

            // удалить отсутствующие в облаке по каждому IssueId
            foreach (var group in groups)
            {
                // выбрать только Id к удалению, без трекинга сущностей
                List<TimeEntry> toDeleteEntries = await unitOfWork.TimeEntry.GetMissingFromCloudAsync(group.IssueId, group.CloudIds, ct);

                if (toDeleteEntries.Count == 0)
                    continue;

                unitOfWork.TimeEntry.DeleteRange(toDeleteEntries);
            }

            await unitOfWork.SaveChangesAsync(ct);
        }

        private enum TimeEntryDataSource
        {
            Realtime,
            SqlSnapshot
        }
    }
}
