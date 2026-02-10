using CRMService.Abstractions.Database.Repository;
using CRMService.API;
using CRMService.DataBase.Postgresql;
using CRMService.Models.ConfigClass;
using CRMService.Models.Constants;
using CRMService.Models.OkdeskEntity;
using Microsoft.Extensions.Options;
using System.Data;

namespace CRMService.Service.OkdeskEntity
{
    public class TimeEntryService(IOptions<ApiEndpointOptions> endpoint, IOptions<OkdeskOptions> okdesk, GetOkdeskEntityService request, IUnitOfWork unitOfWork, PGSelect pGSelect, ILogger<TimeEntryService> logger)
    {
        public async Task<TimeEntries?> GetimeEntriesFromCloudApi(int issueId, CancellationToken ct)
        {
            string link = $"{endpoint.Value.OkdeskApi}/issues/{issueId}/time_entries?api_token={okdesk.Value.OkdeskApiToken}";

            return await request.GetItem<TimeEntries>(link, ct);
        }

        public async Task UpdateTimeEntriesFromCloudApi(int issueId, CancellationToken ct)
        {
            logger.LogInformation("[Method:{MethodName}] Starting to update time entries for issues from API.", nameof(UpdateTimeEntriesFromCloudApi));

            TimeEntries? timeEntry = await GetimeEntriesFromCloudApi(issueId, ct);

            if (timeEntry == null || timeEntry.Time_Entries == null || timeEntry.Time_Entries.Length == 0)
            {
                List<TimeEntry> timeEntries = await unitOfWork.TimeEntry.GetItemsByPredicateAsync(t => t.IssueId == issueId, asNoTracking: true, ct: ct);
                unitOfWork.TimeEntry.DeleteRange(timeEntries);

                if (timeEntries.Count > 0)
                    await unitOfWork.SaveAsync(ct);

                return;
            }

            foreach (TimeEntry entry in timeEntry.Time_Entries)
            {
                entry.EmployeeId = entry.Employee?.Id ?? throw new InvalidOperationException($"Employee id is not set in time entry: {entry.Id}");
                entry.IssueId = issueId;

                if (await unitOfWork.Employee.GetItemByIdAsync(entry.EmployeeId, asNoTracking: true, ct: ct) == null)
                {
                    logger.LogWarning("[Method:{MethodName}] Employee: {employeeId} - not found in local DB.", nameof(UpdateTimeEntriesFromCloudApi), entry.EmployeeId);
                    continue;
                }

                entry.Employee = null;
                entry.Issue = null;

                TimeEntry? existingEntry = await unitOfWork.TimeEntry.GetItemByIdAsync(entry.Id, ct: ct);

                if (existingEntry == null)
                    unitOfWork.TimeEntry.Create(entry);
                else
                    existingEntry.CopyData(entry);
            }

            await unitOfWork.SaveAsync(ct);

            await DeleteMarkedAsDeletedTimeEntries(timeEntry.Time_Entries, ct);
        }

        private async Task<List<TimeEntry>> GetTimeEntriesFromCloudDb(DateTime dateFrom, DateTime dateTo, long startId, long limit, CancellationToken ct)
        {
            string sqlCommand = string.Format(
                    "SELECT time_entries.id, users.sequential_id AS employee_id, time_entries.spent_time, issues.sequential_id AS issue_id, time_entries.logged_at, time_entries.created_at " +
                    "FROM time_entries " +
                    "LEFT OUTER JOIN users ON time_entries.employee_id = users.id " +
                    "LEFT OUTER JOIN issues ON time_entries.issue_id = issues.id " +
                    "WHERE (time_entries.logged_at BETWEEN '{0}' AND '{1}') " +
                    "AND time_entries.id > {2} " +
                    "ORDER BY time_entries.id LIMIT {3};",
                    dateFrom.ToString("yyyy-MM-dd HH:mm:ss"), dateTo.ToString("yyyy-MM-dd HH:mm:ss"), startId, limit);

            DataSet ds = await pGSelect.Select(sqlCommand, ct);
            DataTable? table = ds.Tables["Table"];
            if (table == null)
                return new();

            return table.AsEnumerable().
                Select(entry => new TimeEntry
                {
                    Id = entry.Field<int>("id"),
                    EmployeeId = entry.Field<int>("employee_id"),
                    SpentTime = entry.Field<double>("spent_time"),
                    IssueId = entry.Field<int>("issue_id"),
                    LoggedAt = entry.Field<DateTime>("logged_at").ToLocalTime(),
                    CreatedAt = entry.Field<DateTime>("created_at").ToLocalTime()
                }).ToList();
        }

        public async Task UpdateTimeEntriesFromCloudDb(DateTime dateFrom, DateTime dateTo, CancellationToken ct)
        {
            logger.LogInformation("[Method:{MethodName}] Starting to update time entries from DB.", nameof(UpdateTimeEntriesFromCloudDb));

            long startId = 0;

            while (true)
            {
                List<TimeEntry> entries = await GetTimeEntriesFromCloudDb(dateFrom, dateTo, startId, LimitConstants.LIMIT_FOR_RETRIEVING_ENTITIES_FROM_DB, ct);

                if (entries.Count == 0)
                    return;

                foreach (TimeEntry item in entries)
                {
                    Issue? existingIssue = await unitOfWork.Issue.GetItemByIdAsync(item.IssueId, asNoTracking: true, ct: ct);

                    if (existingIssue == null)
                    {
                        logger.LogWarning("[Method:{MethodName}] Issue: {issueId} - not found in local DB.", nameof(UpdateTimeEntriesFromCloudDb), item.IssueId);
                        continue;
                    }

                    TimeEntry? existingEntry = await unitOfWork.TimeEntry.GetItemByIdAsync(item.Id, ct: ct);

                    if (existingEntry == null)
                        unitOfWork.TimeEntry.Create(item);
                    else
                        existingEntry.CopyData(item);
                }

                await unitOfWork.SaveAsync(ct);

                startId = entries.Last().Id;

                if (entries.Count < LimitConstants.LIMIT_FOR_RETRIEVING_ENTITIES_FROM_DB)
                    break;
            }

            logger.LogInformation("[Method:{MethodName}] Time entries update completed.", nameof(UpdateTimeEntriesFromCloudDb));
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

            await unitOfWork.SaveAsync(ct);
        }
    }
}