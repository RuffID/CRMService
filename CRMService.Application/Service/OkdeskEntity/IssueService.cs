using CRMService.Application.Abstractions.Database.Repository;
using CRMService.Application.Models.ConfigClass;
using CRMService.Application.Service.Sync;
using CRMService.Domain.Models.OkdeskEntity;
using Microsoft.Extensions.Options;
using System.Runtime.CompilerServices;

namespace CRMService.Application.Service.OkdeskEntity
{
    public class IssueService(IOptions<ApiEndpointOptions> endpoint, IOptions<OkdeskOptions> okdeskSettings, IOkdeskEntityRequestService itemService, IUnitOfWork unitOfWork, IOkdeskUnitOfWork okdeskUnitOfWork, EntitySyncService sync, ILogger<IssueService> logger)
    {
        private async IAsyncEnumerable<List<Issue>> GetIssuesFromCloudApi(DateTime updatedSinceFrom, DateTime updatedUntilTo, int assigneeId, long pageNumber, long startIndex, long limit, [EnumeratorCancellation] CancellationToken ct)
        {
            string link = string.Format("{0}/issues/list?api_token={1}&updated_since={2}&updated_until={3}&assignee_ids[]={4}",
                    endpoint.Value.OkdeskApi, okdeskSettings.Value.OkdeskApiToken, updatedSinceFrom.ToString("dd-MM-yyyy HH:mm:ss"), updatedUntilTo.ToString("dd-MM-yyyy HH:mm:ss"), assigneeId);

            await foreach (List<Issue> issues in itemService.GetAllItemsAsync<Issue>(link, startIndex, limit, pageNumber, ct))
                yield return issues;
        }

        private async Task<List<Issue>> GetIssuesFromCloudDb(DateTime updatedSinceFrom, DateTime updatedUntilTo, int startIndex, int limit, CancellationToken ct)
        {
            List<Issue> issues = await okdeskUnitOfWork.Issue.GetUpdatedItemsAsync(updatedSinceFrom, updatedUntilTo, startIndex, limit, ct);

            return issues.OrderBy(x => x.Id).ToList();
        }

        public async Task UpdateIssuesFromCloudApi(DateTime dateFrom, DateTime dateTo, long startIndex = 0, long limit = 0, [CallerMemberName] string caller = "", CancellationToken ct = default)
        {
            logger.LogInformation("[Method:{MethodName}][Caller:{CallerMethod}] Starting updating issues.", nameof(UpdateIssuesFromCloudApi), caller);

            long employeeStartIndex = 0;
            List<Employee> employees = await unitOfWork.Employee.GetItemsByPredicateAsync(predicate: e => e.Id >= employeeStartIndex, asNoTracking: true, ct: ct);

            if (employees.Count == 0)
                return;

            long pageNubmer = 1;

            while (employees.Count != 0)
            {
                foreach (Employee employee in employees.Where(e => e.Active == true))
                {
                    await foreach (List<Issue> issues in GetIssuesFromCloudApi(dateFrom, dateTo, employee.Id, pageNubmer, startIndex, limit, ct))
                    {
                        foreach (Issue issue in issues)
                        {
                            issue.AssigneeId = employee.Id;

                            await sync.RunExclusive(issue, async () =>
                            {
                                await CreateOrUpdate(issue, ct);
                            }, ct);
                        }
                    }
                }

                employeeStartIndex = employees.Last().Id;

                employees = await unitOfWork.Employee.GetItemsByPredicateAsync(predicate: e => e.Id > employeeStartIndex, asNoTracking: true, ct: ct);
            }

            logger.LogInformation("[Method:{MethodName}][Caller:{CallerMethod}] Issues update completed.", nameof(UpdateIssuesFromCloudApi), caller);
        }

        public async Task UpdateIssuesFromCloudDb(DateTime dateFrom, DateTime dateTo, int startIndex, int limit, [CallerMemberName] string caller = "", CancellationToken ct = default)
        {
            logger.LogInformation("[Method:{MethodName}][Caller:{CallerMethod}] Starting to update issues.", nameof(UpdateIssuesFromCloudDb), caller);

            while (true)
            {
                List<Issue> issues = await GetIssuesFromCloudDb(dateFrom, dateTo, startIndex, limit, ct);

                if (issues.Count == 0)
                    break;

                foreach (Issue issue in issues)
                {
                    await sync.RunExclusive(issue, async () =>
                    {
                        await CreateOrUpdate(issue, ct);
                    }, ct);
                }

                startIndex = issues.Last().Id;

                if (issues.Count < limit)
                    break;
            }

            logger.LogInformation("[Method:{MethodName}][Caller:{CallerMethod}] Issues update completed.", nameof(UpdateIssuesFromCloudDb), caller);
        }

        public async Task CreateOrUpdate(Issue issue, CancellationToken ct)
        {
            await CheckAttributes(issue, ct);

            Issue? existingIssue = await unitOfWork.Issue.GetItemByIdAsync(issue.Id, ct: ct);
            if (existingIssue == null)
                unitOfWork.Issue.Create(issue);
            else
                existingIssue.CopyData(issue);

            await unitOfWork.SaveChangesAsync(ct);
        }

        public async Task CheckAttributes(Issue issue, CancellationToken ct)
        {
            if (issue.Company != null)
            {
                Company? company = await unitOfWork.Company.GetItemByIdAsync(issue.Company.Id, true, ct: ct);
                if (company == null)
                {
                    logger.LogWarning("[Method:{MethodName}] Company with id: {CompanyId} was not found for issue with id: {IssueId}.",
                        nameof(CheckAttributes), issue.Company.Id, issue.Id);
                    issue.CompanyId = null;
                }
                else
                {
                    issue.CompanyId = company?.Id;
                }
            }
            else if (issue.CompanyId.HasValue)
            {
                Company? company = await unitOfWork.Company.GetItemByIdAsync(issue.CompanyId.Value, true, ct: ct);
                if (company == null)
                {
                    logger.LogWarning("[Method:{MethodName}] Company with id: {CompanyId} was not found for issue with id: {IssueId}.",
                        nameof(CheckAttributes), issue.CompanyId, issue.Id);
                    issue.CompanyId = null;
                }
                else
                {
                    issue.CompanyId = company?.Id;
                }
            }

            if (issue.ServiceObject != null)
            {
                MaintenanceEntity? serviceObject = await unitOfWork.MaintenanceEntity.GetItemByIdAsync(issue.ServiceObject.Id, true, ct: ct);
                if (serviceObject == null)
                {
                    logger.LogWarning("[Method:{MethodName}] Service object with id: {ServiceObjectId} was not found for issue with id: {IssueId}.",
                        nameof(CheckAttributes), issue.ServiceObject.Id, issue.Id);
                    issue.ServiceObjectId = null;
                }
                else
                {
                    issue.ServiceObjectId = serviceObject?.Id;
                }
            }
            else if (issue.ServiceObjectId.HasValue)
            {
                MaintenanceEntity? serviceObject = await unitOfWork.MaintenanceEntity.GetItemByIdAsync(issue.ServiceObjectId.Value, true, ct: ct);
                if (serviceObject == null)
                {
                    logger.LogWarning("[Method:{MethodName}] Service object with id: {ServiceObjectId} was not found for issue with id: {IssueId}.",
                        nameof(CheckAttributes), issue.ServiceObjectId, issue.Id);
                    issue.ServiceObjectId = null;
                }
                else
                {
                    issue.ServiceObjectId = serviceObject?.Id;
                }
            }

            if (issue.Status != null)
            {
                IssueStatus? status = await unitOfWork.IssueStatus.GetItemByPredicateAsync(s => s.Code == issue.Status.Code, true, ct: ct);
                if (status == null)
                {
                    logger.LogWarning("[Method:{MethodName}] Status with code: {StatusCode} was not found for issue with id: {IssueId}.",
                        nameof(CheckAttributes), issue.Status.Code, issue.Id);
                    issue.StatusId = null;
                }
                else
                {
                    issue.StatusId = status?.Id;
                }
            }

            if (issue.Priority != null)
            {
                IssuePriority? priority = await unitOfWork.IssuePriority.GetItemByPredicateAsync(p => p.Code == issue.Priority.Code, true, ct: ct);
                if (priority == null)
                {
                    logger.LogWarning("[Method:{MethodName}] Priority with code: {PriorityCode} was not found for issue with id: {IssueId}.",
                        nameof(CheckAttributes), issue.Priority.Code, issue.Id);
                    issue.PriorityId = null;
                }
                else
                {
                    issue.PriorityId = priority?.Id;
                }
            }

            if (issue.Type != null)
            {
                IssueType? type = await unitOfWork.IssueType.GetItemByPredicateAsync(t => t.Code == issue.Type.Code, true, ct: ct);
                if (type == null)
                {
                    logger.LogWarning("[Method:{MethodName}] Type with code: {TypeCode} was not found for issue with id: {IssueId}.",
                        nameof(CheckAttributes), issue.Type.Code, issue.Id);
                    issue.TypeId = null;
                }
                else
                {
                    issue.TypeId = type?.Id;
                }
            }

            if (issue.AssigneeId != null && issue.AssigneeId != 0)
            {
                Employee? assignee = await unitOfWork.Employee.GetItemByIdAsync(issue.AssigneeId.Value, true, ct: ct);
                if (assignee == null)
                {
                    logger.LogWarning("[Method:{MethodName}] Employee with id: {AssigneeId} was not found for issue with id: {IssueId}.",
                        nameof(CheckAttributes), issue.AssigneeId, issue.Id);
                    issue.AssigneeId = null;
                }
                else
                {
                    issue.AssigneeId = assignee?.Id;
                }
            }

            issue.Company = null;
            issue.ServiceObject = null;
            issue.Status = null;
            issue.Priority = null;
            issue.Type = null;
            issue.Assignee = null;
        }
    }
}
