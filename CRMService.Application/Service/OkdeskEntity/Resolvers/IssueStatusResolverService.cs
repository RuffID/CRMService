using CRMService.Application.Abstractions.Database.Repository;
using CRMService.Domain.Models.OkdeskEntity;

namespace CRMService.Application.Service.OkdeskEntity.Resolvers
{
    public class IssueStatusResolverService(
        IUnitOfWork unitOfWork,
        IssueStatusService issueStatusService,
        ReferenceResolveHelper referenceResolveHelper,
        ILogger<IssueStatusResolverService> logger)
    {
        public Task<int?> ResolveStatusIdAsync(IssueStatus issueStatus, int issueId, CancellationToken ct)
        {
            ArgumentNullException.ThrowIfNull(issueStatus);

            return referenceResolveHelper.ResolveAsync(
                issueStatus.Code,
                async token => (await unitOfWork.IssueStatus.GetItemByPredicateAsync(s => s.Code == issueStatus.Code, true, ct: token))?.Id,
                issueStatusService.UpdateIssueStatusesFromCloudDb,
                code => $"issue-status:{code}",
                code => $"Status with code: {code} was not found for issue with id: {issueId}. Refreshing statuses from API.",
                code => $"Status with code '{code}' was not found after refresh for issue '{issueId}'.",
                logger,
                nameof(ResolveStatusIdAsync),
                ct);
        }
    }
}