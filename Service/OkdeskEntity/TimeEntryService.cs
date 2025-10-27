using CRMService.API;
using CRMService.DataBase.Postgresql;
using CRMService.Interfaces.Repository;
using CRMService.Models.ConfigClass;
using CRMService.Models.OkdeskEntity;
using Microsoft.Extensions.Options;
using System.Data;

namespace CRMService.Service.OkdeskEntity
{
    public class TimeEntryService(IOptions<ApiEndpointOptions> endpoint, IOptions<OkdeskOptions> okdesk, GetOkdeskEntityService request, IUnitOfWork unitOfWork, PGSelect pGSelect, ILoggerFactory logger)
    {
        private readonly ILogger<TimeEntryService> _logger = logger.CreateLogger<TimeEntryService>();

        public async Task<TimeEntries?> GetimeEntriesFromCloudApi(int issueId)
        {
            string link = $"{endpoint.Value.OkdeskApi}/issues/{issueId}/time_entries?api_token={okdesk.Value.OkdeskApiToken}";

            return await request.GetItem<TimeEntries>(link);
        }

        public async Task UpdateTimeEntriesFromCloudApi(int issueId, CancellationToken ct)
        {
            TimeEntries? timeEntry = await GetimeEntriesFromCloudApi(issueId);

            if (timeEntry == null || timeEntry.Time_Entries == null || timeEntry.Time_Entries.Length == 0)
            {
                await unitOfWork.TimeEntry.DeleteAllByIssueId(issueId, ct); // удалить всё по IssueId если вдруг что то было в БД по этой заявке
                await unitOfWork.SaveAsync(ct);
                return;
            }

            foreach (TimeEntry entry in timeEntry.Time_Entries)
            {
                entry.IssueId = issueId;
                entry.EmployeeId = entry.Employee?.Id ?? throw new InvalidOperationException($"Employee id is not set in time entry: {entry.Id}");
            }

            await unitOfWork.TimeEntry.Upsert(timeEntry.Time_Entries, ct);

            await unitOfWork.SaveAsync(ct);

            await DeleteMarkedAsDeletedTimeEntries(timeEntry.Time_Entries, ct);
        }

        private async Task<List<TimeEntry>?> GetTimeEntriesFromCloudDb(DateTime dateFrom, DateTime dateTo, long timeEntryId, long limit)
        {
            string sqlCommand = string.Format(
                    "SELECT time_entries.id, users.sequential_id AS employee_id, time_entries.spent_time, issues.sequential_id AS issue_id, time_entries.logged_at, time_entries.created_at " +
                    "FROM time_entries " +
                    "LEFT OUTER JOIN users ON time_entries.employee_id = users.id " +
                    "LEFT OUTER JOIN issues ON time_entries.issue_id = issues.id " +
                    "WHERE (time_entries.logged_at BETWEEN '{0}' AND '{1}') " +
                    "AND time_entries.id > '{2}' ORDER BY time_entries.id LIMIT '{3}';",
                    dateFrom.ToString("yyyy-MM-dd HH:mm:ss"), dateTo.ToString("yyyy-MM-dd HH:mm:ss"), timeEntryId, limit);

            DataSet ds = await pGSelect.Select(sqlCommand);
            DataTable? table = ds.Tables["Table"];
            if (table == null)
                return null;

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

        public async Task UpdateTimeEntriesFromCloudDb(DateTime dateFrom, DateTime dateTo, long startIndex, long limit, CancellationToken ct)
        {
            _logger.LogInformation("[Method:{MethodName}] Starting updating time entries.", nameof(UpdateTimeEntriesFromCloudDb));

            while (true)
            {
                List<TimeEntry>? entries = await GetTimeEntriesFromCloudDb(dateFrom, dateTo, startIndex, limit);

                if (entries == null || entries.Count == 0)
                    return;

                startIndex = entries.Last().Id;

                await unitOfWork.TimeEntry.Upsert(entries, ct);
                await unitOfWork.SaveAsync(ct);

                if (entries.Count < limit)
                    break;
            }

            _logger.LogInformation("[Method:{MethodName}] Time entries update completed.", nameof(UpdateTimeEntriesFromCloudDb));
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
                List<int> toDeleteIds = await unitOfWork.TimeEntry.GetItemIdsByCloudIdsFromIssueId(group.IssueId, group.CloudIds, ct);

                if (toDeleteIds.Count == 0)
                    continue;

                IEnumerable<TimeEntry> stubs = toDeleteIds.Select(id => new TimeEntry { Id = id });

                unitOfWork.TimeEntry.DeleteRange(stubs);
            }

            await unitOfWork.SaveAsync(ct);
        }
    }
}