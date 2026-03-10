using CRMService.Application.Abstractions.Database.Repository;
using CRMService.Domain.Models.OkdeskEntity;

namespace CRMService.Application.Service.OkdeskEntity.Resolvers
{
    public class CompanyResolverService(
        IUnitOfWork unitOfWork,
        CompanyService companyService,
        ReferenceResolveHelper referenceResolveHelper,
        ILogger<CompanyResolverService> logger)
    {
        public Task<int> ResolveCompanyIdAsync(Company company, int issueId, CancellationToken ct)
        {
            ArgumentNullException.ThrowIfNull(company);

            return ResolveCompanyIdAsync(company.Id, issueId, ct);
        }

        public Task<int> ResolveCompanyIdAsync(int companyId, int issueId, CancellationToken ct)
        {
            return referenceResolveHelper.ResolveAsync(
                companyId,
                async token => (await unitOfWork.Company.GetItemByIdAsync(companyId, true, ct: token))?.Id,
                token => companyService.UpdateCompanyFromCloudApi(companyId, token),
                id => $"company:{id}",
                id => $"Company with id: {id} was not found for issue with id: {issueId}. Refreshing company from API.",
                id => $"Company with id '{id}' was not found after refresh for issue '{issueId}'.",
                logger,
                nameof(ResolveCompanyIdAsync),
                ct);
        }
    }
}