using CRMService.Application.Abstractions.Database.Repository;
using CRMService.Application.Models.ConfigClass;
using CRMService.Application.Service.Sync;
using CRMService.Domain.Models.Constants;
using CRMService.Domain.Models.OkdeskEntity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Runtime.CompilerServices;

namespace CRMService.Application.Service.OkdeskEntity
{
    public class CompanyService(IOptions<ApiEndpointOptions> endpoint, IOptions<OkdeskOptions> okdSettings,
        IOkdeskEntityRequestService request, IUnitOfWork unitOfWork, IOkdeskUnitOfWork okdeskUnitOfWork, EntitySyncService sync, ILogger<CompanyService> logger)
    {
        public async Task<Company?> GetCompanyFromCloudApi(int companyId)
        {
            string link = $"{endpoint.Value.OkdeskApi}/companies?api_token={okdSettings.Value.OkdeskApiToken}&id={companyId}";

            return await request.GetItemAsync<Company>(link);
        }

        private async IAsyncEnumerable<List<Company>> GetCompaniesFromCloudApiByCategory(IEnumerable<CompanyCategory> categories, long limit, [EnumeratorCancellation] CancellationToken ct)
        {
            foreach (CompanyCategory category in categories)
            {
                string link = $"{endpoint.Value.OkdeskApi}/companies/list?api_token={okdSettings.Value.OkdeskApiToken}&category_ids[]={category.Id}";
                await foreach (List<Company> companies in request.GetAllItemsAsync<Company>(link, startIndex: 0, limit, ct: ct))
                {
                    foreach (Company company in companies)
                    {
                        company.CategoryId = null;
                        company.Category = new CompanyCategory
                        {
                            Code = category.Code
                        };
                    }

                    yield return companies;
                }
            }
        }

        private async Task<List<Company>> GetCompaniesFromCloudDb(CancellationToken ct)
        {
            List<Company> companies = await okdeskUnitOfWork.Company.GetItemsByPredicateAsync(
                asNoTracking: true,
                include: query => query.Include(x => x.Category),
                ct: ct);

            return companies.OrderBy(x => x.Id).ToList();
        }

        public async Task UpdateCompanyFromCloudApi(int companyId, CancellationToken ct)
        {
            Company? company = await GetCompanyFromCloudApi(companyId);

            if (company == null)
                return;

            await sync.RunExclusive(company, async () =>
            {
                await CreateOrUpdateAsync(company, ct);
            }, ct);
        }

        public async Task UpdateCompaniesFromCloudApi(CancellationToken ct)
        {
            logger.LogInformation("[Method:{MethodName}] Starting to update companies from API.", nameof(UpdateCompaniesFromCloudApi));

            List<CompanyCategory> categories = await unitOfWork.CompanyCategory.GetItemsByPredicateAsync(asNoTracking: true, ct: ct);

            if (categories.Count != 0)
            {
                await foreach (List<Company> companies in GetCompaniesFromCloudApiByCategory(categories, LimitConstants.LIMIT_FOR_RETRIEVING_ENTITIES_FROM_API, ct))
                {
                    foreach (Company company in companies)
                    {
                        await sync.RunExclusive(company, async () =>
                        {
                            await CreateOrUpdateAsync(company, ct);
                        }, ct);
                    }
                }
            }

            logger.LogInformation("[Method:{MethodName}] Update companies completed.", nameof(UpdateCompaniesFromCloudApi));
        }

        public async Task UpdateCompaniesFromCloudDb(CancellationToken ct)
        {
            logger.LogInformation("[Method:{MethodName}] Starting to update companies from DB.", nameof(UpdateCompaniesFromCloudDb));

            List<Company> companies = await GetCompaniesFromCloudDb(ct);

            if (companies.Count != 0)
            {
                foreach (Company company in companies)
                {
                    await sync.RunExclusive(company, async () =>
                    {
                        await CreateOrUpdateAsync(company, ct);
                    }, ct);
                }
            }

            logger.LogInformation("[Method:{MethodName}] Companies update completed.", nameof(UpdateCompaniesFromCloudDb));
        }

        public async Task CreateOrUpdateAsync(Company company, CancellationToken ct)
        {
            await CheckCompanyCategory(company, ct);

            Company? existingCompany = await unitOfWork.Company.GetItemByIdAsync(company.Id, ct: ct);

            if (existingCompany == null)
                unitOfWork.Company.Create(company);
            else
                existingCompany.CopyData(company);

            await unitOfWork.SaveChangesAsync(ct);
        }

        public async Task CheckCompanyCategory(Company company, CancellationToken ct)
        {
            if (company.Category == null)
            {
                company.CategoryId = null;
                return;
            }

            CompanyCategory? category = await unitOfWork.CompanyCategory.GetItemByPredicateAsync(c => c.Code == company.Category.Code, ct: ct);
            company.CategoryId = category?.Id;

            company.Category = null;
        }
    }
}