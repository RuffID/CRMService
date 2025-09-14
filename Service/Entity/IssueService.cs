using CRMService.API;
using CRMService.DataBase.Postgresql;
using CRMService.Interfaces.Repository;
using CRMService.Models.ConfigClass;
using CRMService.Models.Entity;
using Microsoft.Extensions.Options;
using System.Data;
using System.Runtime.CompilerServices;

namespace CRMService.Service.Entity
{
    public class IssueService(IOptions<ApiEndpointOptions> endpoint, IOptions<OkdeskOptions> okdeskSettings, GetOkdeskEntityService itemService, 
        IUnitOfWork unitOfWork, PGSelect pGSelect, ILoggerFactory logger)
    {
        private readonly ILogger<IssueService> _logger = logger.CreateLogger<IssueService>();

        private async IAsyncEnumerable<List<Issue>> GetIssuesFromCloudApi(DateTime updatedSinceFrom, DateTime updatedUntilTo, int assigneeId, long pageNumber, long startIndex, long limit)
        {
            string link = string.Format("{0}/issues/list?api_token={1}&updated_since={2}&updated_until={3}&assignee_ids[]={4}",
                    endpoint.Value.OkdeskApi, okdeskSettings.Value.OkdeskApiToken, updatedSinceFrom.ToString("dd-MM-yyyy HH:mm:ss"), updatedUntilTo.ToString("dd-MM-yyyy HH:mm:ss"), assigneeId);

            await foreach (List<Issue> issues in itemService.GetAllItems<Issue>(link, startIndex, limit, pageNumber))
                yield return issues;
        }

        private async Task<List<Issue>?> GetIssuesFromCloudDb(DateTime updatedSinceFrom, DateTime updatedUntilTo, int startIndex, int limit)
        {
            string sqlCommand = string.Format(
                    "SELECT issues.sequential_id, assigned.sequential_id AS assignee_id, author.sequential_id AS author_id, issues.title, issues.created_at, issues.completed_at, issues.deadline_at, issues.employees_updated_at, issues.deleted_at, issues.delay_to, issue_statuses.id AS statusId, issue_work_types.id AS typeId, issue_priorities.id AS priorityId, companies.sequential_id AS companyId, company_maintenance_entities.sequential_id AS maintenanceEntityId " +
                    "FROM issues " +
                    "LEFT OUTER JOIN issue_statuses ON issues.status_id = issue_statuses.id " +
                    "LEFT OUTER JOIN issue_work_types ON issues.work_type_id = issue_work_types.id " +
                    "LEFT OUTER JOIN issue_priorities ON issues.priority_id = issue_priorities.id " +
                    "LEFT OUTER JOIN companies ON issues.company_id = companies.id " +
                    "LEFT OUTER JOIN company_maintenance_entities ON issues.maintenance_entity_id = company_maintenance_entities.id " +
                    "LEFT OUTER JOIN users AS assigned ON issues.assignee_id = assigned.id " +
                    "LEFT OUTER JOIN users AS author ON issues.author_id = author.id " +
                    "WHERE ((issues.employees_updated_at BETWEEN '{0}' AND '{1}') OR (issues.deleted_at BETWEEN '{0}' AND '{1}')) " +
                    "AND issues.sequential_id > '{2}' ORDER BY issues.sequential_id LIMIT '{3}';",
                    updatedSinceFrom.ToString("yyyy-MM-dd HH:mm:ss"), updatedUntilTo.ToString("yyyy-MM-dd HH:mm:ss"), startIndex, limit);

            DataSet ds = await pGSelect.Select(sqlCommand);
            DataTable? issuesTable = ds.Tables["Table"];
            if (issuesTable == null)
                return null;

            return issuesTable.AsEnumerable().
                Select(issue => new Issue
                {
                    Id = issue.Field<int>("sequential_id"),
                    Title = issue.Field<string>("title") ?? "",
                    AssigneeId = issue.Field<int?>("assignee_id"),
                    AuthorId = issue.Field<int>("author_id"),
                    CompanyId = issue.Field<int?>("companyid"),
                    ServiceObjectId = issue.Field<int?>("maintenanceEntityId"),
                    StatusId = issue.Field<int>("statusId"),
                    TypeId = issue.Field<int>("typeId"),
                    PriorityId = issue.Field<int>("priorityId"),
                    CreatedAt = issue.Field<DateTime>("created_at").ToLocalTime(),
                    CompletedAt = issue.Field<DateTime?>("completed_at")?.ToLocalTime(),
                    DeadlineAt = issue.Field<DateTime?>("deadline_at")?.ToLocalTime(),
                    DelayTo = issue.Field<DateTime?>("delay_to")?.ToLocalTime(),
                    EmployeesUpdatedAt = issue.Field<DateTime>("employees_updated_at").ToLocalTime(),
                    DeletedAt = issue.Field<DateTime?>("deleted_at")?.ToLocalTime()
                }).ToList();
        }

        public async Task CheckAttributes(Issue issue, CancellationToken ct)
        {
            issue.StatusId = (await unitOfWork.IssueStatus.GetItemById(issue.Status.Id, true, ct))?.Id 
                ?? throw new Exception($"StatusId: {issue.StatusId} - not found in DB.");
            issue.PriorityId = (await unitOfWork.IssuePriority.GetItemById(issue.Priority.Id, true, ct))?.Id 
                ?? throw new Exception($"PriorityId: {issue.PriorityId} - not found in DB.");
            issue.TypeId = (await unitOfWork.IssueType.GetItemById(issue.Type.Id, true, ct))?.Id 
                ?? throw new Exception($"TypeId: {issue.TypeId} - not found in DB.");

            if (issue.AssigneeId != null)
                issue.AssigneeId = (await unitOfWork.Employee.GetItemById(issue.AssigneeId.Value, true, ct))?.Id;
        }

        public async Task UpdateIssuesFromCloudApi(DateTime dateFrom, DateTime dateTo, long startIndex = 0, long limit = 0, [CallerMemberName] string caller = "", CancellationToken ct = default)
        {
            _logger.LogInformation("[Method:{MethodName}][Caller:{CallerMethod}] Starting updating issues.", nameof(UpdateIssuesFromCloudApi), caller);

            List<Employee> employees = await unitOfWork.Employee.GetItemsByPredicateAndSortById(predicate: e => e.Id >= startIndex, asNoTracking: true, ct: ct);

            if (employees.Count == 0)
                return;

            long pageNubmer = 1;

            foreach (Employee employee in employees.Where(e => e.Active == true))
            {
                await foreach (List<Issue>? issues in GetIssuesFromCloudApi(dateFrom, dateTo, employee.Id, pageNubmer, startIndex, limit))
                {
                    if (issues == null || issues.Count == 0)
                        break;

                    foreach (Issue issue in issues)
                    {
                        issue.AssigneeId = employee.Id;
                        await CheckAttributes(issue, ct);
                        issue.TimeEntries = new List<TimeEntry>();

                        await unitOfWork.Issue.Upsert(issue, ct);
                        await unitOfWork.SaveAsync(ct);
                    }
                }
            }

            _logger.LogInformation("[Method:{MethodName}][Caller:{CallerMethod}] Issues update completed.", nameof(UpdateIssuesFromCloudApi), caller);
        }

        public async Task UpdateIssuesFromCloudDb(DateTime dateFrom, DateTime dateTo, int startIndex, int limit, [CallerMemberName] string caller = "", CancellationToken ct = default)
        {
            _logger.LogInformation("[Method:{MethodName}][Caller:{CallerMethod}] Starting updating issues.", nameof(UpdateIssuesFromCloudDb), caller);

            while (true)
            {
                List<Issue>? issues = await GetIssuesFromCloudDb(dateFrom, dateTo, startIndex, limit);

                if (issues == null || issues.Count == 0)
                    break;

                foreach (Issue issue in issues)
                {
                    await CheckAttributes(issue, ct);

                    await unitOfWork.Issue.Upsert(issue, ct);
                    await unitOfWork.SaveAsync(ct);
                }

                startIndex = issues.Last().Id;

                if (issues.Count < limit)
                    break;
            }

            _logger.LogInformation("[Method:{MethodName}][Caller:{CallerMethod}] Issues update completed.", nameof(UpdateIssuesFromCloudDb), caller);
        }
    }
}