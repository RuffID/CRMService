using CRMService.Application.Abstractions.Database.Repository;
using CRMService.Application.Models.ConfigClass;
using CRMService.Application.Service.OkdeskEntity.Resolvers;
using CRMService.Application.Service.Sync;
using CRMService.Domain.Models.OkdeskEntity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Runtime.CompilerServices;

namespace CRMService.Application.Service.OkdeskEntity
{
    public class IssueService(
        IOptions<ApiEndpointOptions> endpoint,
        IOptions<OkdeskOptions> okdeskSettings,
        IOkdeskEntityRequestService itemService,
        IUnitOfWork unitOfWork,
        IOkdeskUnitOfWork okdeskUnitOfWork,
        EntitySyncService sync,
        CompanyResolverService companyResolver,
        EmployeeResolverService employeeResolver,
        IssuePriorityResolverService issuePriorityResolver,
        IssueStatusResolverService issueStatusResolver,
        IssueTypeResolverService issueTypeResolver,
        MaintenanceEntityResolverService maintenanceEntityResolver,
        ILogger<IssueService> logger)
    {
        private async IAsyncEnumerable<List<Issue>> GetIssuesFromCloudApiAsync(DateTime updatedSinceFrom, DateTime updatedUntilTo, int assigneeId, long pageNumber, long startIndex, long limit, [EnumeratorCancellation] CancellationToken ct)
        {
            string link = string.Format("{0}/issues/list?api_token={1}&updated_since={2}&updated_until={3}&assignee_ids[]={4}",
                    endpoint.Value.OkdeskApi, okdeskSettings.Value.OkdeskApiToken, updatedSinceFrom.ToString("dd-MM-yyyy HH:mm:ss"), updatedUntilTo.ToString("dd-MM-yyyy HH:mm:ss"), assigneeId);

            await foreach (List<Issue> issues in itemService.GetAllItemsAsync<Issue>(link, startIndex, limit, pageNumber, ct))
                yield return issues;
        }

        private async Task<List<Issue>> GetIssuesFromCloudDbAsync(DateTime updatedSinceFrom, DateTime updatedUntilTo, int startIndex, int limit, CancellationToken ct)
        {
            List<Issue> issues = await okdeskUnitOfWork.Issue.GetUpdatedItemsAsync(updatedSinceFrom, updatedUntilTo, startIndex, limit, ct);

            return issues.OrderBy(x => x.Id).ToList();
        }

        public async Task UpdateIssuesFromCloudApiAsync(DateTime dateFrom, DateTime dateTo, long startIndex = 0, long limit = 0, [CallerMemberName] string caller = "", CancellationToken ct = default)
        {
            logger.LogInformation("[Method:{MethodName}][Caller:{CallerMethod}] Starting updating issues.", nameof(UpdateIssuesFromCloudApiAsync), caller);

            long employeeStartIndex = 0;
            HashSet<int> processedIssueIds = new();
            List<Employee> employees = await unitOfWork.Employee.GetItemsByPredicateAsync(predicate: e => e.Id >= employeeStartIndex && e.Active, asNoTracking: true, ct: ct);

            if (employees.Count == 0)
                return;

            long pageNubmer = 1;

            while (employees.Count != 0)
            {
                foreach (Employee employee in employees)
                {
                    await foreach (List<Issue> issues in GetIssuesFromCloudApiAsync(dateFrom, dateTo, employee.Id, pageNubmer, startIndex, limit, ct))
                    {
                        List<Issue> uniqueIssues = issues.Where(issue => processedIssueIds.Add(issue.Id))
                            .ToList();

                        if (uniqueIssues.Count == 0)
                            continue;

                        foreach (Issue issue in uniqueIssues)
                            issue.AssigneeId = employee.Id;

                        IssueBatchContext batchContext = await CreateIssueBatchContextAsync(uniqueIssues, ct);

                        await ProcessIssueBatchAsync(uniqueIssues, batchContext, ct);
                    }
                }

                employeeStartIndex = employees.Last().Id;

                employees = await unitOfWork.Employee.GetItemsByPredicateAsync(predicate: e => e.Id > employeeStartIndex && e.Active, asNoTracking: true, ct: ct);
            }

            logger.LogInformation("[Method:{MethodName}][Caller:{CallerMethod}] Issues update completed.", nameof(UpdateIssuesFromCloudApiAsync), caller);
        }

        public async Task UpdateIssuesFromCloudDbAsync(DateTime dateFrom, DateTime dateTo, int startIndex, int limit, [CallerMemberName] string caller = "", CancellationToken ct = default)
        {
            logger.LogInformation("[Method:{MethodName}][Caller:{CallerMethod}] Starting to update issues.", nameof(UpdateIssuesFromCloudDbAsync), caller);

            while (true)
            {
                List<Issue> issues = await GetIssuesFromCloudDbAsync(dateFrom, dateTo, startIndex, limit, ct);

                if (issues.Count == 0)
                    break;

                IssueBatchContext batchContext = await CreateIssueBatchContextAsync(issues, ct);

                await ProcessIssueBatchAsync(issues, batchContext, ct);

                startIndex = issues.Last().Id;

                if (issues.Count < limit)
                    break;
            }

            logger.LogInformation("[Method:{MethodName}][Caller:{CallerMethod}] Issues update completed.", nameof(UpdateIssuesFromCloudDbAsync), caller);
        }

        public async Task CreateOrUpdateAsync(Issue issue, CancellationToken ct)
        {
            await CheckAttributesAsync(issue, ct);

            Issue? existingIssue = await unitOfWork.Issue.GetItemByIdAsync(issue.Id, ct: ct);
            if (existingIssue == null)
                unitOfWork.Issue.Create(issue);
            else
                existingIssue.CopyData(issue);

            await unitOfWork.SaveChangesAsync(ct);
        }

        private async Task ProcessIssueBatchAsync(List<Issue> issues, IssueBatchContext batchContext, CancellationToken ct)
        {
            HashSet<int> issueIds = issues.Select(issue => issue.Id).ToHashSet();

            List<Issue> existingIssues = await unitOfWork.Issue.GetItemsByPredicateAsync(predicate: issue => issueIds.Contains(issue.Id), ct: ct);

            Dictionary<int, Issue> existingIssuesById = existingIssues.ToDictionary(issue => issue.Id);

            foreach (Issue issue in issues)
            {
                await sync.RunExclusive(issue, async () =>
                {
                    await CheckAttributesAsync(issue, batchContext, ct);

                    if (!existingIssuesById.TryGetValue(issue.Id, out Issue? existingIssue))
                    {
                        unitOfWork.Issue.Create(issue);
                        existingIssuesById[issue.Id] = issue;
                        return;
                    }

                    existingIssue.CopyData(issue);
                }, ct);
            }

            await unitOfWork.SaveChangesAsync(ct);
        }

        public async Task CheckAttributesAsync(Issue issue, CancellationToken ct)
        {
            if (issue.Company != null)
                issue.CompanyId = await companyResolver.ResolveCompanyIdAsync(issue.Company, issue.Id, ct);
            else if (issue.CompanyId.HasValue)
                issue.CompanyId = await companyResolver.ResolveCompanyIdAsync(issue.CompanyId.Value, issue.Id, ct);

            if (issue.ServiceObject != null)
                issue.ServiceObjectId = await maintenanceEntityResolver.ResolveMaintenanceEntityIdAsync(issue.ServiceObject, issue.Id, ct);
            else if (issue.ServiceObjectId.HasValue)
                issue.ServiceObjectId = await maintenanceEntityResolver.ResolveMaintenanceEntityIdAsync(issue.ServiceObjectId.Value, issue.Id, ct);

            if (issue.Status != null)
                issue.StatusId = await issueStatusResolver.ResolveStatusIdAsync(issue.Status, issue.Id, ct);

            if (issue.Priority != null)            
                issue.PriorityId = await issuePriorityResolver.ResolvePriorityIdAsync(issue.Priority, issue.Id, ct);
            
            if (issue.Type != null)
                issue.TypeId = await issueTypeResolver.ResolveTypeIdAsync(issue.Type, issue.Id, ct);

            if (issue.AssigneeId != null && issue.AssigneeId != 0)
                issue.AssigneeId = await employeeResolver.ResolveEmployeeIdAsync(issue.AssigneeId.Value, issue.Id, ct);

            if (issue.AuthorId != null && issue.AuthorId != 0)
                issue.AuthorId = await ResolveAuthorIdAsync(issue.AuthorId.Value, issue.Id, ct);

            issue.Company = null;
            issue.ServiceObject = null;
            issue.Status = null;
            issue.Priority = null;
            issue.Type = null;
            issue.Assignee = null;
        }

        private async Task CheckAttributesAsync(Issue issue, IssueBatchContext batchContext, CancellationToken ct)
        {
            if (issue.Company != null)
            {
                issue.CompanyId = batchContext.CompanyIds.Contains(issue.Company.Id)
                    ? issue.Company.Id
                    : await companyResolver.ResolveCompanyIdAsync(issue.Company, issue.Id, ct);
                if (issue.CompanyId.HasValue)
                    batchContext.CompanyIds.Add(issue.CompanyId.Value);
            }
            else if (issue.CompanyId.HasValue && !batchContext.CompanyIds.Contains(issue.CompanyId.Value))
            {
                issue.CompanyId = await companyResolver.ResolveCompanyIdAsync(issue.CompanyId.Value, issue.Id, ct);
                if (issue.CompanyId.HasValue)
                    batchContext.CompanyIds.Add(issue.CompanyId.Value);
            }

            if (issue.ServiceObject != null)
            {
                issue.ServiceObjectId = batchContext.ServiceObjectIds.Contains(issue.ServiceObject.Id)
                    ? issue.ServiceObject.Id
                    : await maintenanceEntityResolver.ResolveMaintenanceEntityIdAsync(issue.ServiceObject, issue.Id, ct);
                if (issue.ServiceObjectId.HasValue)
                    batchContext.ServiceObjectIds.Add(issue.ServiceObjectId.Value);
            }
            else if (issue.ServiceObjectId.HasValue && !batchContext.ServiceObjectIds.Contains(issue.ServiceObjectId.Value))
            {
                issue.ServiceObjectId = await maintenanceEntityResolver.ResolveMaintenanceEntityIdAsync(issue.ServiceObjectId.Value, issue.Id, ct);
                if (issue.ServiceObjectId.HasValue)
                    batchContext.ServiceObjectIds.Add(issue.ServiceObjectId.Value);
            }

            if (issue.Status != null)
            {
                string statusCode = issue.Status.Code;
                int? statusId = batchContext.StatusIdsByCode.TryGetValue(statusCode, out int existingStatusId)
                    ? existingStatusId
                    : await issueStatusResolver.ResolveStatusIdAsync(issue.Status, issue.Id, ct);

                if (statusId.HasValue)
                    batchContext.StatusIdsByCode[statusCode] = statusId.Value;

                issue.StatusId = statusId;
            }

            if (issue.Priority != null)
            {
                string priorityCode = issue.Priority.Code;
                int? priorityId = batchContext.PriorityIdsByCode.TryGetValue(priorityCode, out int existingPriorityId)
                    ? existingPriorityId
                    : await issuePriorityResolver.ResolvePriorityIdAsync(issue.Priority, issue.Id, ct);

                if (priorityId.HasValue)
                    batchContext.PriorityIdsByCode[priorityCode] = priorityId.Value;

                issue.PriorityId = priorityId;
            }

            if (issue.Type != null)
            {
                string typeCode = issue.Type.Code;
                int? typeId = batchContext.TypeIdsByCode.TryGetValue(typeCode, out int existingTypeId)
                    ? existingTypeId
                    : await issueTypeResolver.ResolveTypeIdAsync(issue.Type, issue.Id, ct);

                if (typeId.HasValue)
                    batchContext.TypeIdsByCode[typeCode] = typeId.Value;

                issue.TypeId = typeId;
            }

            if (issue.AssigneeId != null && issue.AssigneeId != 0 && !batchContext.EmployeeIds.Contains(issue.AssigneeId.Value))
            {
                issue.AssigneeId = await employeeResolver.ResolveEmployeeIdAsync(issue.AssigneeId.Value, issue.Id, ct);
                if (issue.AssigneeId.HasValue)
                    batchContext.EmployeeIds.Add(issue.AssigneeId.Value);
            }

            if (issue.AuthorId != null && issue.AuthorId != 0)
            {
                if (batchContext.ContactAuthorIds.Contains(issue.AuthorId.Value))
                    issue.AuthorId = null;
                else if (!batchContext.EmployeeIds.Contains(issue.AuthorId.Value))
                {
                    issue.AuthorId = await ResolveAuthorIdAsync(issue.AuthorId.Value, issue.Id, ct);
                    if (issue.AuthorId.HasValue)
                        batchContext.EmployeeIds.Add(issue.AuthorId.Value);
                }
            }

            issue.Company = null;
            issue.ServiceObject = null;
            issue.Status = null;
            issue.Priority = null;
            issue.Type = null;
            issue.Assignee = null;
        }

        private async Task<IssueBatchContext> CreateIssueBatchContextAsync(List<Issue> issues, CancellationToken ct)
        {
            HashSet<int> companyIds = issues
                .Select(issue => issue.Company?.Id ?? issue.CompanyId)
                .Where(id => id.HasValue)
                .Select(id => id!.Value)
                .ToHashSet();

            HashSet<int> serviceObjectIds = issues
                .Select(issue => issue.ServiceObject?.Id ?? issue.ServiceObjectId)
                .Where(id => id.HasValue)
                .Select(id => id!.Value)
                .ToHashSet();

            HashSet<int> employeeIds = issues
                .SelectMany(issue => new int?[] { issue.AssigneeId, issue.AuthorId })
                .Where(id => id.HasValue && id.Value != 0)
                .Select(id => id!.Value)
                .ToHashSet();

            HashSet<int> authorIds = issues
                .Where(issue => issue.AuthorId.HasValue && issue.AuthorId.Value != 0)
                .Select(issue => issue.AuthorId!.Value)
                .ToHashSet();

            HashSet<string> statusCodes = issues
                .Where(issue => issue.Status != null && !string.IsNullOrWhiteSpace(issue.Status.Code))
                .Select(issue => issue.Status!.Code)
                .ToHashSet(StringComparer.Ordinal);

            HashSet<string> typeCodes = issues
                .Where(issue => issue.Type != null && !string.IsNullOrWhiteSpace(issue.Type.Code))
                .Select(issue => issue.Type!.Code)
                .ToHashSet(StringComparer.Ordinal);

            HashSet<string> priorityCodes = issues
                .Where(issue => issue.Priority != null && !string.IsNullOrWhiteSpace(issue.Priority.Code))
                .Select(issue => issue.Priority!.Code)
                .ToHashSet(StringComparer.Ordinal);

            List<Company> companies = companyIds.Count == 0
                ? new()
                : await unitOfWork.Company.GetItemsByPredicateAsync(company => companyIds.Contains(company.Id), asNoTracking: true, ct: ct);

            List<MaintenanceEntity> serviceObjects = serviceObjectIds.Count == 0
                ? new()
                : await unitOfWork.MaintenanceEntity.GetItemsByPredicateAsync(serviceObject => serviceObjectIds.Contains(serviceObject.Id), asNoTracking: true, ct: ct);

            List<Employee> employees = employeeIds.Count == 0
                ? new()
                : await unitOfWork.Employee.GetItemsByPredicateAsync(employee => employeeIds.Contains(employee.Id), asNoTracking: true, ct: ct);

            List<IssueStatus> statuses = statusCodes.Count == 0
                ? new()
                : await unitOfWork.IssueStatus.GetItemsByPredicateAsync(status => statusCodes.Contains(status.Code), asNoTracking: true, ct: ct);

            List<IssueType> types = typeCodes.Count == 0
                ? new()
                : await unitOfWork.IssueType.GetItemsByPredicateAsync(type => typeCodes.Contains(type.Code), asNoTracking: true, ct: ct);

            List<IssuePriority> priorities = priorityCodes.Count == 0
                ? new()
                : await unitOfWork.IssuePriority.GetItemsByPredicateAsync(priority => priorityCodes.Contains(priority.Code), asNoTracking: true, ct: ct);

            List<Employee> contactAuthors = authorIds.Count == 0
                ? new()
                : await okdeskUnitOfWork.Employee.GetItemsByPredicateAsync(
                    employee => authorIds.Contains(employee.Id) && EF.Property<string>(employee, "Type") == "Contact",
                    asNoTracking: true,
                    ct: ct);

            return new IssueBatchContext(
                companies.Select(company => company.Id).ToHashSet(),
                serviceObjects.Select(serviceObject => serviceObject.Id).ToHashSet(),
                employees.Select(employee => employee.Id).ToHashSet(),
                contactAuthors.Select(employee => employee.Id).ToHashSet(),
                statuses.ToDictionary(status => status.Code, status => status.Id, StringComparer.Ordinal),
                types.ToDictionary(type => type.Code, type => type.Id, StringComparer.Ordinal),
                priorities.ToDictionary(priority => priority.Code, priority => priority.Id, StringComparer.Ordinal));
        }

        private async Task<int?> ResolveAuthorIdAsync(int authorId, int issueId, CancellationToken ct)
        {
            Employee? contactAuthor = await okdeskUnitOfWork.Employee.GetItemByPredicateAsync(
                employee => employee.Id == authorId && EF.Property<string>(employee, "Type") == "Contact",
                asNoTracking: true,
                ct: ct);

            if (contactAuthor != null)
                return null;

            return await employeeResolver.ResolveEmployeeIdAsync(authorId, issueId, ct);
        }

        private sealed class IssueBatchContext(
            HashSet<int> companyIds,
            HashSet<int> serviceObjectIds,
            HashSet<int> employeeIds,
            HashSet<int> contactAuthorIds,
            Dictionary<string, int> statusIdsByCode,
            Dictionary<string, int> typeIdsByCode,
            Dictionary<string, int> priorityIdsByCode)
        {
            public HashSet<int> CompanyIds { get; } = companyIds;
            public HashSet<int> ServiceObjectIds { get; } = serviceObjectIds;
            public HashSet<int> EmployeeIds { get; } = employeeIds;
            public HashSet<int> ContactAuthorIds { get; } = contactAuthorIds;
            public Dictionary<string, int> StatusIdsByCode { get; } = statusIdsByCode;
            public Dictionary<string, int> TypeIdsByCode { get; } = typeIdsByCode;
            public Dictionary<string, int> PriorityIdsByCode { get; } = priorityIdsByCode;
        }
    }
}
