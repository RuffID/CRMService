using CRMService.Application.Abstractions.Database.Repository;
using CRMService.Application.Common.Mapping.OkdeskEntity;
using CRMService.Application.Models.ConfigClass;
using CRMService.Application.Service.Sync;
using CRMService.Contracts.Models.Dto.Lookup;
using CRMService.Contracts.Models.Dto.OkdeskEntity;
using CRMService.Contracts.Models.Request;
using CRMService.Contracts.Models.Responses.Results;
using CRMService.Domain.Models.Constants;
using CRMService.Domain.Models.OkdeskEntity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Runtime.CompilerServices;
using CRMService.Application.Abstractions.Service;
using Microsoft.Extensions.Logging;

namespace CRMService.Application.Service.OkdeskEntity
{
    public class CompanyService(IOptions<ApiEndpointOptions> endpoint, IOptions<OkdeskOptions> okdSettings,
        IOkdeskEntityRequestService request, IUnitOfWork unitOfWork, IOkdeskUnitOfWork okdeskUnitOfWork, EntitySyncService sync, ILogger<CompanyService> logger)
    {
        private const int DEFAULT_LOOKUP_LIMIT = 20;

        public async Task<ServiceResult<List<CompanyDto>>> GetCompaniesAsync(CancellationToken ct = default)
        {
            List<Company> companies = await unitOfWork.Company.GetItemsByPredicateAsync(asNoTracking: true, ct: ct);
            return ServiceResult<List<CompanyDto>>.Ok(companies.OrderBy(company => company.Name).ThenBy(company => company.Id).ToDto().ToList());
        }

        public async Task<ServiceResult<List<LookupOptionDto>>> GetCompanyLookupAsync(LookupListRequest requestModel, CancellationToken ct = default)
        {
            ServiceResult validationResult = ValidateLookupRequest(requestModel);
            if (!validationResult.Success)
                return ServiceResult<List<LookupOptionDto>>.Fail(validationResult.Error!.StatusCode, validationResult.Error.Message);

            string? normalizedSearch = NormalizeSearch(requestModel.Search);

            List<Company> companies = await unitOfWork.Company.GetItemsByPredicateAsync(
                predicate: company => normalizedSearch == null
                    || company.Name.Contains(normalizedSearch)
                    || (company.AdditionalName != null && company.AdditionalName.Contains(normalizedSearch)),
                asNoTracking: true,
                include: query => query.Include(company => company.Category),
                ct: ct);

            IEnumerable<Company> orderedCompanies = normalizedSearch == null
                ? companies.OrderBy(company => company.Id)
                : companies
                    .OrderBy(company => GetSearchRank(company.Name, normalizedSearch, company.AdditionalName))
                    .ThenBy(company => company.Name)
                    .ThenBy(company => company.Id);

            List<LookupOptionDto> items = orderedCompanies
                .Skip(requestModel.Offset)
                .Take(requestModel.Limit)
                .Select(company => new LookupOptionDto
                {
                    Id = company.Id,
                    Text = FormatCompanyText(company),
                    Color = company.Category?.Color ?? string.Empty
                })
                .ToList();

            return ServiceResult<List<LookupOptionDto>>.Ok(items);
        }

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

        private static ServiceResult ValidateLookupRequest(LookupListRequest request)
        {
            if (request.Offset < 0)
                return ServiceResult.Fail(400, "Смещение не может быть отрицательным.");

            if (request.Limit == 0)
                request.Limit = DEFAULT_LOOKUP_LIMIT;

            if (request.Limit <= 0 || request.Limit > 100)
                return ServiceResult.Fail(400, "Лимит должен быть в диапазоне от 1 до 100.");

            return ServiceResult.Ok();
        }

        private static string? NormalizeSearch(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            string normalized = value.Trim();
            int nonWhitespaceCount = normalized.Count(character => !char.IsWhiteSpace(character));
            return nonWhitespaceCount >= 2 ? normalized : null;
        }

        private static string FormatCompanyText(Company company)
        {
            if (string.IsNullOrWhiteSpace(company.AdditionalName))
                return company.Name;

            return $"{company.Name} ({company.AdditionalName})";
        }

        private static int GetSearchRank(string value, string search, string? extraValue = null)
        {
            if (value.StartsWith(search, StringComparison.OrdinalIgnoreCase))
                return 0;

            if (!string.IsNullOrWhiteSpace(extraValue) && extraValue.StartsWith(search, StringComparison.OrdinalIgnoreCase))
                return 1;

            if (value.Contains(search, StringComparison.OrdinalIgnoreCase))
                return 2;

            if (!string.IsNullOrWhiteSpace(extraValue) && extraValue.Contains(search, StringComparison.OrdinalIgnoreCase))
                return 3;

            return 4;
        }
    }
}
