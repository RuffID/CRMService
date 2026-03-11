using CRMService.Application.Abstractions.Database.Repository;
using CRMService.Domain.Models.OkdeskEntity;

namespace CRMService.Application.Service.OkdeskEntity.Resolvers
{
    public class IssuePriorityResolverService(
        IUnitOfWork unitOfWork, 
        IssuePriorityService issuePriorityService, 
        ReferenceResolveHelper referenceResolveHelper,
        ILogger<IssuePriorityResolverService> logger)
    {
        public Task<int?> ResolvePriorityIdAsync(IssuePriority issuePriority, int issueId, CancellationToken ct)
        {
            ArgumentNullException.ThrowIfNull(issuePriority);

            return referenceResolveHelper.ResolveAsync(
                issuePriority.Code,
                async token => (await unitOfWork.IssuePriority.GetItemByPredicateAsync(p => p.Code == issuePriority.Code, true, ct: token))?.Id,
                issuePriorityService.UpdateIssuePrioritiesFromCloudDb,
                code => $"issue-priority:{code}",
                code => $"Priority with code: {code} was not found for issue with id: {issueId}. Refreshing priorities from API.",
                code => $"Priority with code '{code}' was not found after refresh for issue '{issueId}'.",
                logger,
                nameof(ResolvePriorityIdAsync),
                ct);
        }
    }
}