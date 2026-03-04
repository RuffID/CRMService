using CRMService.Application.Abstractions.Database.Repository;
using CRMService.Application.Models.ConfigClass;
using CRMService.Application.Service.Sync;
using CRMService.Domain.Models.Constants;
using CRMService.Domain.Models.OkdeskEntity;
using Microsoft.Extensions.Options;
using System.Data;
using System.Runtime.CompilerServices;

namespace CRMService.Application.Service.OkdeskEntity
{
    public class CompanyService(IOptions<ApiEndpointOptions> endpoint, IOptions<OkdeskOptions> okdSettings,
        IOkdeskEntityRequestService request, IUnitOfWork unitOfWork, IPostgresSelect postgresSelect, EntitySyncService sync, ILogger<CompanyService> logger)
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
                        company.CategoryId = category.Id;
                        company.Category = null;
                    }

                    yield return companies;
                }
            }
        }

        private async IAsyncEnumerable<List<Company>> GetCompaniesFromCloudDbByCategory(IEnumerable<CompanyCategory> categories, [EnumeratorCancellation] CancellationToken ct)
        {
            foreach (CompanyCategory category in categories)
            {
                while (true)
                {
                    string sqlCommand = string.Format(
                        "SELECT companies.sequential_id AS id, companies.name, companies.additional_name, companies.active " +
                        "FROM companies " +
                        "LEFT OUTER JOIN company_categories ON companies.category_id = company_categories.id");

                    if (category.Code == "no_category")
                        sqlCommand += " AND company_categories.code IS NULL ";
                    else sqlCommand += $" AND company_categories.code = '{category.Code}' ";

                    sqlCommand += $" ORDER BY companies.sequential_id LIMIT {LimitConstants.LIMIT_FOR_RETRIEVING_ENTITIES_FROM_DB};";

                    DataSet ds = await postgresSelect.Select(sqlCommand, ct);
                    DataTable? companyTable = ds.Tables["Table"];
                    if (companyTable == null)
                        break;

                    List<Company> companies = companyTable.AsEnumerable().
                        Select(company => new Company
                        {
                            Id = company.Field<int>("id"),
                            Name = company.Field<string>("name") ?? string.Empty,
                            AdditionalName = company.Field<string>("additional_name"),
                            CategoryId = category.Id,
                            Active = company.Field<bool>("active"),
                            Equipment = null,
                            MaintenanceEntities = null,
                            Issues = null
                        }).ToList();

                    if (companies == null || companies.Count == 0)
                        break;

                    yield return companies;
                }
            }
        }

        public async Task UpdateCompanyFromCloudApi(int companyId, CancellationToken ct)
        {
            Company? company = await GetCompanyFromCloudApi(companyId);

            if (company == null)
                return;

            await sync.RunExclusive(company, async () =>
            {
                if (!await CheckCompanyCategory(company, ct))
                    return;

                Company? existingCompany = await unitOfWork.Company.GetItemByIdAsync(company.Id, ct: ct);

                if (existingCompany == null)
                    unitOfWork.Company.Create(company);
                else
                    existingCompany.CopyData(company);

                await unitOfWork.SaveChangesAsync(ct);
            }, ct);
        }

        public async Task UpdateCompaniesFromCloudApi(CancellationToken ct)
        {
            logger.LogInformation("[Method:{MethodName}] Starting to update companies from API.", nameof(UpdateCompaniesFromCloudApi));

            List<CompanyCategory> categories = await unitOfWork.CompanyCategory.GetItemsByPredicateAsync(asNoTracking: true, ct: ct);

            if (categories.Count == 0)
                return;

            await foreach (List<Company> companies in GetCompaniesFromCloudApiByCategory(categories, LimitConstants.LIMIT_FOR_RETRIEVING_ENTITIES_FROM_API, ct))
            {
                foreach (Company company in companies)
                {
                    await sync.RunExclusive(company, async () =>
                    {
                        Company? existingCompany = await unitOfWork.Company.GetItemByIdAsync(company.Id, ct: ct);
                        if (existingCompany == null)
                            unitOfWork.Company.Create(company);
                        else
                            existingCompany.CopyData(company);

                        await unitOfWork.SaveChangesAsync(ct);
                    }, ct);
                }
            }

            logger.LogInformation("[Method:{MethodName}] Update companies completed.", nameof(UpdateCompaniesFromCloudApi));
        }

        public async Task UpdateCompaniesFromCloudDb(CancellationToken ct)
        {
            logger.LogInformation("[Method:{MethodName}] Starting to update companies from DB.", nameof(UpdateCompaniesFromCloudDb));

            IEnumerable<CompanyCategory> categories = await unitOfWork.CompanyCategory.GetItemsByPredicateAsync(asNoTracking: true, ct: ct);

            if (categories == null || !categories.Any())
                return;

            await foreach (List<Company> companies in GetCompaniesFromCloudDbByCategory(categories, ct))
            {
                if (companies.Count == 0)
                    continue;

                foreach (Company company in companies)
                {
                    await sync.RunExclusive(company, async () =>
                    {
                        Company? existingCompany = await unitOfWork.Company.GetItemByIdAsync(company.Id, ct: ct);
                        if (existingCompany == null)
                            unitOfWork.Company.Create(company);
                        else
                            existingCompany.CopyData(company);

                        await unitOfWork.SaveChangesAsync(ct);
                    }, ct);
                }
            }

            logger.LogInformation("[Method:{MethodName}] Companies update completed.", nameof(UpdateCompaniesFromCloudDb));
        }

        public async Task<bool> CheckCompanyCategory(Company company, CancellationToken ct)
        {
            if (company.Category == null)
            {
                company.CategoryId = 0;
                return true;
            }

            CompanyCategory? category = await unitOfWork.CompanyCategory.GetItemByIdAsync(company.Category.Id, ct: ct);
            if (category != null)
            {
                company.CategoryId = category.Id;
                company.Category = null;
                return true;
            }

            company.Category = null;
            return false;
        }
    }
}