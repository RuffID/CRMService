using CRMService.Application.Abstractions.Database.Repository;
using CRMService.Application.Models.ConfigClass;
using CRMService.Application.Service.OkdeskEntity.Resolvers;
using CRMService.Application.Service.Sync;
using CRMService.Domain.Models.OkdeskEntity;
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
            List<Employee> employees = await unitOfWork.Employee.GetItemsByPredicateAsync(predicate: e => e.Id >= employeeStartIndex, asNoTracking: true, ct: ct);

            if (employees.Count == 0)
                return;

            long pageNubmer = 1;

            while (employees.Count != 0)
            {
                foreach (Employee employee in employees.Where(e => e.Active == true))
                {
                    await foreach (List<Issue> issues in GetIssuesFromCloudApiAsync(dateFrom, dateTo, employee.Id, pageNubmer, startIndex, limit, ct))
                    {
                        foreach (Issue issue in issues)
                        {
                            issue.AssigneeId = employee.Id;

                            await sync.RunExclusive(issue, async () =>
                            {
                                await CreateOrUpdateAsync(issue, ct);
                            }, ct);
                        }
                    }
                }

                employeeStartIndex = employees.Last().Id;

                employees = await unitOfWork.Employee.GetItemsByPredicateAsync(predicate: e => e.Id > employeeStartIndex, asNoTracking: true, ct: ct);
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

                foreach (Issue issue in issues)
                {
                    await sync.RunExclusive(issue, async () =>
                    {
                        await CreateOrUpdateAsync(issue, ct);
                    }, ct);
                }

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

            issue.Company = null;
            issue.ServiceObject = null;
            issue.Status = null;
            issue.Priority = null;
            issue.Type = null;
            issue.Assignee = null;
        }
    }
}
