using CRMService.Application.Abstractions.Database.Repository;
using CRMService.Application.Common.Exceptions;
using CRMService.Application.Models.ConfigClass;
using CRMService.Domain.Models.Constants;
using CRMService.Domain.Models.OkdeskEntity;
using Microsoft.Extensions.Options;

namespace CRMService.Application.Service.OkdeskEntity
{
    public class TimeEntryService(IOptions<ApiEndpointOptions> endpoint, IOptions<OkdeskOptions> okdesk, IOkdeskEntityRequestService request, IUnitOfWork unitOfWork, IOkdeskUnitOfWork okdeskUnitOfWork, ILogger<TimeEntryService> logger)
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

                List<TimeEntry> existingTimeEntries = await unitOfWork.TimeEntry.GetItemsByPredicateAsync(t => t.IssueId == issueId, asNoTracking: true, ct: ct);

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
                List<TimeEntry> existingTimeEntries = await unitOfWork.TimeEntry.GetItemsByPredicateAsync(t => t.IssueId == issueId, asNoTracking: true, ct: ct);

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

                if (await unitOfWork.Employee.GetItemByIdAsync(entry.EmployeeId, asNoTracking: true, ct: ct) == null)
                {
                    logger.LogWarning("[Method:{MethodName}] Employee {EmployeeId} not found in local DB.", 
                        nameof(UpdateTimeEntriesFromCloudApi), entry.EmployeeId);

                    continue;
                }

                entry.Employee = null;
                entry.Issue = null;

                await CreateOrUpdate(entry, ct);
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
                    await CreateOrUpdate(item, ct);                

                startId = entries.Last().Id;

                if (entries.Count < LimitConstants.LIMIT_FOR_RETRIEVING_ENTITIES_FROM_DB)
                    break;
            }

            logger.LogInformation("[Method:{MethodName}] Time entries update completed.", nameof(UpdateTimeEntriesFromCloudDb));
        }

        public async Task CreateOrUpdate(TimeEntry entry, CancellationToken ct)
        {
            Issue? existingIssue = await unitOfWork.Issue.GetItemByIdAsync(entry.IssueId, asNoTracking: true, ct: ct);

            if (existingIssue == null)
            {
                logger.LogWarning("[Method:{MethodName}] Issue: {issueId} - not found in local DB.", nameof(CreateOrUpdate), entry.IssueId);
                return;
            }

            TimeEntry? existingEntry = await unitOfWork.TimeEntry.GetItemByIdAsync(entry.Id, ct: ct);

            if (existingEntry == null)
                unitOfWork.TimeEntry.Create(entry);
            else
                existingEntry.CopyData(entry);

            await unitOfWork.SaveChangesAsync(ct);
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
                List<TimeEntry> toDeleteEntries = await unitOfWork.TimeEntry.GetItemsByPredicateAsync(t => t.IssueId == group.IssueId && !group.CloudIds.Contains(t.Id), ct: ct);

                if (toDeleteEntries.Count == 0)
                    continue;

                unitOfWork.TimeEntry.DeleteRange(toDeleteEntries);
            }

            await unitOfWork.SaveChangesAsync(ct);
        }
    }
}