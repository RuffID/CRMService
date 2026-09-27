using CRMService.Application.Abstractions.Database.Repository;
using CRMService.Application.Models.ConfigClass;
using CRMService.Application.Models.OkdeskApi;
using CRMService.Application.Service.OkdeskEntity.Resolvers;
using CRMService.Application.Service.Sync;
using CRMService.Contracts.Models.Dto.OkdeskEntity;
using CRMService.Contracts.Models.Request;
using CRMService.Contracts.Models.Responses.Results;
using CRMService.Domain.Models.OkdeskEntity;
using Microsoft.Extensions.Options;
using System.Runtime.CompilerServices;
using System.Text;
using CRMService.Application.Abstractions.Service;
using Microsoft.Extensions.Logging;

namespace CRMService.Application.Service.OkdeskEntity
{
    public class IssueService(
        IOptions<ApiEndpointOptions> endpoint,
        IOptions<OkdeskOptions> okdeskSettings,
        IOkdeskEntityRequestService itemService,
        IIssuesUnitOfWork unitOfWork,
        IOkdeskIssuesSource okdeskUnitOfWork,
        EntitySyncService sync,
        CompanyResolverService companyResolver,
        EmployeeResolverService employeeResolver,
        IssuePriorityResolverService issuePriorityResolver,
        IssueStatusResolverService issueStatusResolver,
        IssueTypeResolverService issueTypeResolver,
        MaintenanceEntityResolverService maintenanceEntityResolver,
        ILogger<IssueService> logger)
    {
        public Task<List<Issue>> GetIssuesAsync(int startIndex, int limit, CancellationToken ct = default) =>
            unitOfWork.Issue.GetFromIdReadOnlyAsync(startIndex, limit, ct);

        public Task<Issue?> GetIssueAsync(int id, CancellationToken ct = default) =>
            unitOfWork.Issue.GetItemByIdReadOnlyAsync(id, ct);

        public Task<List<Issue>> GetUpdatedLocalIssuesAsync(DateTime dateFrom, DateTime dateTo, CancellationToken ct = default) =>
            unitOfWork.Issue.GetUpdatedLocalReadOnlyAsync(dateFrom, dateTo, ct);

        private const int MAX_PAGE_SIZE = 100;
        private const int QUICK_COUNT_LIMIT = 1000;
        private const int QUICK_COUNT_QUERY_LIMIT = QUICK_COUNT_LIMIT + 1;
        private const int ISSUE_EXISTENCE_BATCH_SIZE = 50;
        private const int MAX_ISSUE_EXISTENCE_URL_LENGTH = 1800;

        public async Task<ServiceResult<IssueListPageDto>> GetIssueListPageAsync(IssueListRequest request, CancellationToken ct = default)
        {
            ServiceResult normalizedResult = NormalizeIssueListRequest(request);
            if (!normalizedResult.Success)
                return ServiceResult<IssueListPageDto>.Fail(normalizedResult.Error!.StatusCode, normalizedResult.Error.Message);

            int skip = (request.Page - 1) * request.PageSize;
            List<Issue> issues = await unitOfWork.Issue.GetPageByFilterAsync(request, skip, request.PageSize + 1, ct);
            int quickCount = await unitOfWork.Issue.GetCountByFilterAsync(request, QUICK_COUNT_QUERY_LIMIT, ct);

            bool hasNextPage = issues.Count > request.PageSize;
            List<IssueListItemDto> items = issues
                .Take(request.PageSize)
                .Select(MapIssueListItem)
                .ToList();

            bool isTotalCountCapped = quickCount > QUICK_COUNT_LIMIT;
            int displayTotalCount = isTotalCountCapped ? QUICK_COUNT_LIMIT : quickCount;
            int totalPages = CalculateVisibleTotalPages(request.Page, request.PageSize, displayTotalCount, hasNextPage);

            return ServiceResult<IssueListPageDto>.Ok(new IssueListPageDto
            {
                Items = items,
                Page = request.Page,
                PageSize = request.PageSize,
                TotalPages = totalPages,
                DisplayTotalCount = displayTotalCount,
                IsTotalCountCapped = isTotalCountCapped,
                HasNextPage = hasNextPage
            });
        }

        public async Task<ServiceResult<IssueExactCountDto>> GetIssueExactCountAsync(IssueListRequest request, CancellationToken ct = default)
        {
            ServiceResult normalizedResult = NormalizeIssueListRequest(request);
            if (!normalizedResult.Success)
                return ServiceResult<IssueExactCountDto>.Fail(normalizedResult.Error!.StatusCode, normalizedResult.Error.Message);

            int totalCount = await unitOfWork.Issue.GetCountByFilterAsync(request, maxCount: null, ct);

            return ServiceResult<IssueExactCountDto>.Ok(new IssueExactCountDto
            {
                TotalCount = totalCount,
                TotalPages = CalculateTotalPages(totalCount, request.PageSize)
            });
        }

        public async Task<ServiceResult<IssueDetailsDto>> GetIssueDetailsAsync(int id, CancellationToken ct = default)
        {
            if (id <= 0)
                return ServiceResult<IssueDetailsDto>.Fail(400, "Идентификатор заявки должен быть больше нуля.");

            Issue? issue = await unitOfWork.Issue.GetDetailsReadOnlyAsync(id, ct);

            if (issue == null || issue.DeletedAt.HasValue)
                return ServiceResult<IssueDetailsDto>.Fail(404, "Заявка не найдена.");

            Employee? author = null;
            if (issue.AuthorId.HasValue && issue.AuthorId.Value > 0)
                author = await unitOfWork.Employee.GetItemByIdReadOnlyAsync(issue.AuthorId.Value, ct);

            return ServiceResult<IssueDetailsDto>.Ok(new IssueDetailsDto
            {
                Id = issue.Id,
                Title = issue.Title,
                CompanyName = issue.Company?.Name ?? "Не указан",
                CompanyCategoryColor = issue.Company?.Category?.Color ?? string.Empty,
                ServiceObjectName = issue.ServiceObject?.Name ?? "Не указан",
                AssigneeName = FormatEmployeeName(issue.Assignee),
                AuthorName = FormatEmployeeName(author),
                TypeName = issue.Type?.Name ?? "Не указан",
                StatusName = issue.Status?.Name ?? "Не указан",
                StatusColor = issue.Status?.Color ?? string.Empty,
                PriorityName = issue.Priority?.Name ?? "Не указан",
                PriorityColor = issue.Priority?.Color ?? string.Empty,
                CreatedAt = issue.CreatedAt,
                CompletedAt = issue.CompletedAt,
                DeadlineAt = issue.DeadlineAt,
                DelayTo = issue.DelayTo
            });
        }

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
            List<Employee> employees = await unitOfWork.Employee.GetActiveFromIdReadOnlyAsync(employeeStartIndex, inclusive: true, ct);
            long pageNubmer = 1;

            while (employees.Count != 0)
            {
                foreach (Employee employee in employees)
                    await UpdateIssuesForAssigneeAsync(dateFrom, dateTo, employee.Id, pageNubmer, startIndex, limit, processedIssueIds, ct);

                employeeStartIndex = employees.Last().Id;

                employees = await unitOfWork.Employee.GetActiveFromIdReadOnlyAsync(employeeStartIndex, inclusive: false, ct);
            }

            await UpdateIssuesForAssigneeAsync(dateFrom, dateTo, 0, pageNubmer, startIndex, limit, processedIssueIds, ct);

            logger.LogInformation("[Method:{MethodName}][Caller:{CallerMethod}] Issues update completed.", nameof(UpdateIssuesFromCloudApiAsync), caller);
        }

        public async Task ReconcileMissingCurrentIssuesAsync([CallerMemberName] string caller = "", CancellationToken ct = default)
        {
            logger.LogInformation("[Method:{MethodName}][Caller:{CallerMethod}] Starting current issues existence reconciliation.", nameof(ReconcileMissingCurrentIssuesAsync), caller);

            List<int> currentIssueIds = await unitOfWork.Issue.GetCurrentIssueIdsAsync(ct);
            int missingIssuesCount = 0;

            foreach (List<int> issueIds in CreateIssueExistenceBatches(currentIssueIds))
            {
                string link = BuildIssueExistenceListLink(issueIds);
                List<IssueExistenceResponse> existingIssues = await itemService.GetRangeOfItemsAsync<IssueExistenceResponse>(link, ct: ct);
                HashSet<int> existingIssueIds = existingIssues.Select(issue => issue.Id).ToHashSet();

                foreach (int issueId in issueIds.Where(id => !existingIssueIds.Contains(id)))
                {
                    if (!await IsIssueMissingInOkdeskAsync(issueId, ct))
                        continue;

                    await MarkIssueAsDeletedAsync(issueId, ct);
                    missingIssuesCount++;
                }
            }

            logger.LogInformation(
                "[Method:{MethodName}][Caller:{CallerMethod}] Current issues existence reconciliation completed. Checked: {CheckedCount}, marked as deleted: {MissingCount}.",
                nameof(ReconcileMissingCurrentIssuesAsync),
                caller,
                currentIssueIds.Count,
                missingIssuesCount);
        }

        private async Task UpdateIssuesForAssigneeAsync(
            DateTime dateFrom,
            DateTime dateTo,
            int assigneeId,
            long pageNumber,
            long startIndex,
            long limit,
            HashSet<int> processedIssueIds,
            CancellationToken ct)
        {
            await foreach (List<Issue> issues in GetIssuesFromCloudApiAsync(dateFrom, dateTo, assigneeId, pageNumber, startIndex, limit, ct))
            {
                List<Issue> uniqueIssues = issues
                    .Where(issue => processedIssueIds.Add(issue.Id))
                    .ToList();

                if (uniqueIssues.Count == 0)
                    continue;

                foreach (Issue issue in uniqueIssues)
                    issue.AssigneeId = assigneeId == 0 ? null : assigneeId;

                IssueBatchContext batchContext = await CreateIssueBatchContextAsync(uniqueIssues, ct);
                await ProcessIssueBatchAsync(uniqueIssues, batchContext, IssueDataSource.RestApi, ct);
            }
        }

        private IEnumerable<List<int>> CreateIssueExistenceBatches(IReadOnlyCollection<int> issueIds)
        {
            List<int> batch = new(ISSUE_EXISTENCE_BATCH_SIZE);

            foreach (int issueId in issueIds)
            {
                batch.Add(issueId);

                if (batch.Count <= ISSUE_EXISTENCE_BATCH_SIZE && BuildIssueExistenceListLink(batch).Length <= MAX_ISSUE_EXISTENCE_URL_LENGTH)
                    continue;

                batch.RemoveAt(batch.Count - 1);
                if (batch.Count == 0)
                    throw new InvalidOperationException($"Issue ID {issueId} does not fit into the Okdesk existence request URL.");

                yield return batch;

                batch = new List<int>(ISSUE_EXISTENCE_BATCH_SIZE) { issueId };
                if (BuildIssueExistenceListLink(batch).Length > MAX_ISSUE_EXISTENCE_URL_LENGTH)
                    throw new InvalidOperationException($"Issue ID {issueId} does not fit into the Okdesk existence request URL.");
            }

            if (batch.Count != 0)
                yield return batch;
        }

        private string BuildIssueExistenceListLink(IEnumerable<int> issueIds)
        {
            StringBuilder link = new();
            link.Append(endpoint.Value.OkdeskApi)
                .Append("/issues/list?api_token=")
                .Append(okdeskSettings.Value.OkdeskApiToken)
                .Append("&fields[issue]=id&page[size]=")
                .Append(ISSUE_EXISTENCE_BATCH_SIZE)
                .Append("&page[number]=1");

            foreach (int issueId in issueIds)
                link.Append("&ids[]=").Append(issueId);

            return link.ToString();
        }

        private async Task<bool> IsIssueMissingInOkdeskAsync(int issueId, CancellationToken ct)
        {
            await Task.Delay(2000, ct);

            string link = $"{endpoint.Value.OkdeskApi}/issues/{issueId}?api_token={okdeskSettings.Value.OkdeskApiToken}";
            IssueExistenceResponse? response = await itemService.GetItemAsync<IssueExistenceResponse>(link, ct);

            if (response == null)
            {
                logger.LogWarning("[Method:{MethodName}] Could not confirm existence of issue {IssueId}; local issue was not changed.", nameof(IsIssueMissingInOkdeskAsync), issueId);
                return false;
            }

            if (response.Id == issueId)
                return false;

            string expectedError = $"Записи {issueId} не существует";
            if (string.Equals(response.Errors?.Trim(), expectedError, StringComparison.Ordinal))
                return true;

            logger.LogWarning(
                "[Method:{MethodName}] Okdesk returned an unexpected existence response for issue {IssueId}; local issue was not changed. Error: {Error}",
                nameof(IsIssueMissingInOkdeskAsync),
                issueId,
                response.Errors);
            return false;
        }

        private async Task MarkIssueAsDeletedAsync(int issueId, CancellationToken ct)
        {
            Issue syncIssue = new() { Id = issueId };

            await sync.RunExclusive(syncIssue, async () =>
            {
                Issue? issue = await unitOfWork.Issue.GetItemByIdAsync(issueId, ct: ct);
                if (issue == null || issue.DeletedAt.HasValue)
                    return;

                issue.DeletedAt = DateTime.Now;
                await unitOfWork.SaveChangesAsync(ct);

                logger.LogInformation("[Method:{MethodName}] Issue {IssueId} was marked as deleted because Okdesk confirmed that it does not exist.", nameof(MarkIssueAsDeletedAsync), issueId);
            }, ct);
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

                await ProcessIssueBatchAsync(issues, batchContext, IssueDataSource.SqlSnapshot, ct);

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
                Merge(existingIssue, issue, IssueDataSource.Webhook);

            await unitOfWork.SaveChangesAsync(ct);
        }

        private async Task ProcessIssueBatchAsync(
            List<Issue> issues,
            IssueBatchContext batchContext,
            IssueDataSource dataSource,
            CancellationToken ct)
        {
            HashSet<int> issueIds = issues.Select(issue => issue.Id).ToHashSet();

            List<Issue> existingIssues = await unitOfWork.Issue.GetByIdsAsync(issueIds, ct);

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

                    Merge(existingIssue, issue, dataSource);
                }, ct);
            }

            await unitOfWork.SaveChangesAsync(ct);
        }

        private static void Merge(Issue existingIssue, Issue incomingIssue, IssueDataSource dataSource)
        {
            if (dataSource == IssueDataSource.SqlSnapshot
                && incomingIssue.EmployeesUpdatedAt < existingIssue.EmployeesUpdatedAt)
            {
                return;
            }

            if (dataSource == IssueDataSource.RestApi)
            {
                int? groupId = existingIssue.GroupId;
                DateTime employeesUpdatedAt = existingIssue.EmployeesUpdatedAt > incomingIssue.EmployeesUpdatedAt
                    ? existingIssue.EmployeesUpdatedAt
                    : incomingIssue.EmployeesUpdatedAt;

                existingIssue.CopyData(incomingIssue);
                existingIssue.GroupId = groupId;
                existingIssue.EmployeesUpdatedAt = employeesUpdatedAt;
                return;
            }

            existingIssue.CopyData(incomingIssue);
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
                : await unitOfWork.Company.GetByIdsReadOnlyAsync(companyIds, ct);

            List<MaintenanceEntity> serviceObjects = serviceObjectIds.Count == 0
                ? new()
                : await unitOfWork.MaintenanceEntity.GetByIdsReadOnlyAsync(serviceObjectIds, ct);

            List<Employee> employees = employeeIds.Count == 0
                ? new()
                : await unitOfWork.Employee.GetByIdsReadOnlyAsync(employeeIds, ct: ct);

            List<IssueStatus> statuses = statusCodes.Count == 0
                ? new()
                : await unitOfWork.IssueStatus.GetByCodesReadOnlyAsync(statusCodes, ct);

            List<IssueType> types = typeCodes.Count == 0
                ? new()
                : await unitOfWork.IssueType.GetByCodesReadOnlyAsync(typeCodes, ct);

            List<IssuePriority> priorities = priorityCodes.Count == 0
                ? new()
                : await unitOfWork.IssuePriority.GetByCodesReadOnlyAsync(priorityCodes, ct);

            List<Employee> contactAuthors = authorIds.Count == 0
                ? new()
                : await okdeskUnitOfWork.Employee.GetContactsByIdsReadOnlyAsync(authorIds, ct);

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
            Employee? contactAuthor = await okdeskUnitOfWork.Employee.GetContactByIdReadOnlyAsync(authorId, ct);

            if (contactAuthor != null)
                return null;

            return await employeeResolver.ResolveEmployeeIdAsync(authorId, issueId, ct);
        }

        private static ServiceResult NormalizeIssueListRequest(IssueListRequest request)
        {
            if (request.Page <= 0)
                return ServiceResult.Fail(400, "Номер страницы должен быть больше нуля.");

            if (request.PageSize != 20 && request.PageSize != 50 && request.PageSize != MAX_PAGE_SIZE)
                return ServiceResult.Fail(400, "Допустимые размеры страницы: 20, 50, 100.");

            request.AssigneeIds = NormalizeIds(request.AssigneeIds);
            request.AuthorIds = NormalizeIds(request.AuthorIds);
            request.TypeIds = NormalizeIds(request.TypeIds);
            request.StatusIds = NormalizeIds(request.StatusIds);
            request.PriorityIds = NormalizeIds(request.PriorityIds);
            request.CompanyIds = NormalizeIds(request.CompanyIds);
            request.GroupIds = NormalizeIds(request.GroupIds);

            request.NumberFrom = NormalizeIssueNumber(request.NumberFrom);
            request.NumberTo = NormalizeIssueNumber(request.NumberTo);
            request.Search = NormalizeSearch(request.Search);

            if (request.NumberFrom.HasValue && request.NumberTo.HasValue && request.NumberFrom.Value > request.NumberTo.Value)
                return ServiceResult.Fail(400, "Некорректный диапазон номера заявки.");

            request.RegistrationDateFrom = NormalizeDateFrom(request.RegistrationDateFrom);
            request.RegistrationDateTo = NormalizeDateToExclusive(request.RegistrationDateTo);
            request.ResolutionDateFrom = NormalizeDateFrom(request.ResolutionDateFrom);
            request.ResolutionDateTo = NormalizeDateToExclusive(request.ResolutionDateTo);

            if (request.RegistrationDateFrom.HasValue && request.RegistrationDateTo.HasValue && request.RegistrationDateFrom.Value >= request.RegistrationDateTo.Value)
                return ServiceResult.Fail(400, "Некорректный диапазон даты регистрации.");

            if (request.ResolutionDateFrom.HasValue && request.ResolutionDateTo.HasValue && request.ResolutionDateFrom.Value >= request.ResolutionDateTo.Value)
                return ServiceResult.Fail(400, "Некорректный диапазон даты решения.");

            return ServiceResult.Ok();
        }

        private static List<int>? NormalizeIds(List<int>? ids)
        {
            if (ids == null || ids.Count == 0)
                return null;

            List<int> values = ids
                .Where(id => id > 0)
                .Distinct()
                .ToList();

            return values.Count == 0 ? null : values;
        }

        private static int? NormalizeIssueNumber(int? value)
            => value.HasValue && value.Value > 0 ? value.Value : null;

        private static string? NormalizeSearch(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            string normalized = value.Trim();
            int nonWhitespaceCount = normalized.Count(character => !char.IsWhiteSpace(character));
            return nonWhitespaceCount >= 2 ? normalized : null;
        }

        private static DateTime? NormalizeDateFrom(DateTime? value)
            => value?.Date;

        private static DateTime? NormalizeDateToExclusive(DateTime? value)
            => value?.Date.AddDays(1);

        private static IssueListItemDto MapIssueListItem(Issue issue)
        {
            return new IssueListItemDto
            {
                Id = issue.Id,
                Title = issue.Title,
                CompanyName = issue.Company?.Name ?? "Не указан",
                CompanyCategoryColor = issue.Company?.Category?.Color ?? string.Empty,
                AssigneeName = FormatEmployeeName(issue.Assignee),
                CreatedAt = issue.CreatedAt,
                CompletedAt = issue.CompletedAt,
                StatusName = issue.Status?.Name ?? "Не указан",
                StatusColor = issue.Status?.Color ?? string.Empty,
                PriorityName = issue.Priority?.Name ?? "Не указан",
                PriorityColor = issue.Priority?.Color ?? string.Empty
            };
        }

        private static string FormatEmployeeName(Employee? employee)
        {
            if (employee == null)
                return "Не указан";

            string[] parts = new[]
            {
                employee.LastName ?? string.Empty,
                employee.FirstName ?? string.Empty,
                employee.Patronymic ?? string.Empty
            };

            string fullName = string.Join(" ", parts.Where(part => !string.IsNullOrWhiteSpace(part)));
            return string.IsNullOrWhiteSpace(fullName) ? $"#{employee.Id}" : fullName;
        }

        private static int CalculateVisibleTotalPages(int page, int pageSize, int displayTotalCount, bool hasNextPage)
        {
            int totalPages = CalculateTotalPages(displayTotalCount, pageSize);
            int minimalVisiblePages = hasNextPage ? page + 1 : page;
            return Math.Max(totalPages, minimalVisiblePages);
        }

        private static int CalculateTotalPages(int totalCount, int pageSize)
        {
            if (totalCount <= 0)
                return 1;

            return (int)Math.Ceiling(totalCount / (double)pageSize);
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

        private enum IssueDataSource
        {
            RestApi,
            Webhook,
            SqlSnapshot
        }
    }
}
