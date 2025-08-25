using CRMService.API;
using CRMService.DataBase.Postgresql;
using CRMService.Interfaces.Repository;
using CRMService.Models.ConfigClass;
using CRMService.Models.Entity;
using Microsoft.Extensions.Options;
using System.Data;

namespace CRMService.Service.Entity
{
    public class TimeEntryService(IOptions<ApiEndpoint> endpoint, IOptions<OkdeskSettings> okdesk, IOptions<DatabaseSettings> dbSettings, GetItemService request, IUnitOfWork unitOfWork, PGSelect pGSelect, ILoggerFactory logger)
    {
        private readonly ILogger<TimeEntryService> _logger = logger.CreateLogger<TimeEntryService>();

        public async Task<TimeEntries?> GetimeEntriesFromCloudApi(int issueId)
        {
            string link = $"{endpoint.Value.OkdeskApi}/issues/{issueId}/time_entries?api_token={okdesk.Value.ApiToken}";

            return await request.GetItem<TimeEntries>(link);
        }

        public async Task UpdateTimeEntriesFromCloudApi(int issueId)
        {
            TimeEntries? timeEntry = await GetimeEntriesFromCloudApi(issueId);

            if (timeEntry == null || timeEntry.Time_Entries == null || timeEntry.Time_Entries.Length == 0)
                return;

            foreach (TimeEntry entry in timeEntry.Time_Entries)
            {
                entry.IssueId = issueId;
                entry.EmployeeId = entry.Employee?.Id;
                entry.Employee = null;
                await CheckEmployeeAndIssue(entry);
            }

            await unitOfWork.TimeEntry.CreateOrUpdate(timeEntry.Time_Entries);

            await unitOfWork.SaveAsync();

            await DeleteMarkedAsDeketedTimeEntries(timeEntry.Time_Entries);
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
                    EmployeeId = entry.Field<int?>("employee_id"),
                    SpentTime = entry.Field<double?>("spent_time"),
                    IssueId = entry.Field<int?>("issue_id"),
                    LoggedAt = entry.Field<DateTime?>("logged_at")?.ToLocalTime(),
                    CreatedAt = entry.Field<DateTime?>("created_at")?.ToLocalTime()
                }).ToList();
        }

        public async Task UpdateTimeEntriesFromCloudDb(DateTime dateFrom, DateTime dateTo, long indexOfEntries = 0)
        {
            _logger.LogInformation("[Method:{MethodName}] Starting updating time entries.", nameof(UpdateTimeEntriesFromCloudDb));

            while (true)
            {
                List<TimeEntry>? entries = await GetTimeEntriesFromCloudDb(dateFrom, dateTo, indexOfEntries, dbSettings.Value.LimitForRetrievingEntitiesFromDb);

                if (entries == null || entries.Count == 0)
                    return;

                foreach (TimeEntry entry in entries)
                    await CheckEmployeeAndIssue(entry);

                indexOfEntries = entries.Last().Id;

                await unitOfWork.TimeEntry.CreateOrUpdate(entries);
                await unitOfWork.SaveAsync();

                if (entries.Count < dbSettings.Value.LimitForRetrievingEntitiesFromDb)
                    break;
            }

            _logger.LogInformation("[Method:{MethodName}] Time entries update completed.", nameof(UpdateTimeEntriesFromCloudDb));
        }

        private async Task DeleteMarkedAsDeketedTimeEntries(TimeEntry[] entriesFromCloudApi)
        {
            foreach (TimeEntry entry in entriesFromCloudApi)
            {
                if (entry.IssueId == null) continue;
                // Получение всех записей сохранённых в локальной БД сервера
                List<TimeEntry>? timeEntriesFromLocalDB = (await unitOfWork.TimeEntry.GetEntriesByIssue((int)entry.IssueId))?.ToList();

                if (timeEntriesFromLocalDB == null || !timeEntriesFromLocalDB.Any()) continue;

                // Проходит циклом по каждой записи из БД сервера для поиска удалённых 
                foreach (TimeEntry entryFromLocalDB in timeEntriesFromLocalDB)
                {
                    // Если запись из локальной БД есть в записях полученных из API, значит она существует (не удалена) и проверяется следующая запись
                    if (entriesFromCloudApi.Any(te => te.Id == entryFromLocalDB.Id)) continue;

                    // Если запись отсутствует среди записей полученных с API окдеска, то значит она была удалена в окдеске и её нужно удалить и в локальной БД
                    unitOfWork.TimeEntry.Delete(entryFromLocalDB);
                }
            }

            await unitOfWork.SaveAsync();
        }

        public async Task CheckEmployeeAndIssue(TimeEntry entry)
        {
            // Проверяет наличие сотрудника; при отсутствии зануляет
            if (entry.EmployeeId != null)
            {
                if (await unitOfWork.Employee.GetEmployeeById((int)entry.EmployeeId, false) == null)
                    entry.EmployeeId = null;
            }

            // Сохраняет IssueId, если это текущая задача в изменяемом графе
            if (entry.IssueId != null)
            {
                if (await unitOfWork.Issue.GetIssueById((int)entry.IssueId, false) == null)
                    entry.IssueId = null;
            }
        }
    }
}