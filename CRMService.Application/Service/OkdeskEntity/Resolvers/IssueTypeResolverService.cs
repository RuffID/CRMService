using CRMService.Application.Abstractions.Database.Repository;
using CRMService.Domain.Models.OkdeskEntity;

namespace CRMService.Application.Service.OkdeskEntity.Resolvers
{
    public class IssueTypeResolverService(
        IUnitOfWork unitOfWork,
        IssueTypeService issueTypeService,
        ReferenceResolveHelper referenceResolveHelper,
        ILogger<IssueTypeResolverService> logger)
    {
        public Task<int> ResolveTypeIdAsync(IssueType issueType, int issueId, CancellationToken ct)
        {
            ArgumentNullException.ThrowIfNull(issueType);

            return referenceResolveHelper.ResolveAsync(
                issueType.Code,
                async token => (await unitOfWork.IssueType.GetItemByPredicateAsync(t => t.Code == issueType.Code, true, ct: token))?.Id,
                issueTypeService.UpdateIssueTypesFromCloudApi,
                code => $"issue-type:{code}",
                code => $"Type with code: {code} was not found for issue with id: {issueId}. Refreshing issue types from API.",
                code => $"Type with code '{code}' was not found after refresh for issue '{issueId}'.",
                logger,
                nameof(ResolveTypeIdAsync),
                ct);
        }
    }
}