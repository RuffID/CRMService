using CRMService.API;
using CRMService.DataBase.Postgresql;
using CRMService.Interfaces.Repository;
using CRMService.Models.ConfigClass;
using CRMService.Models.Entity;
using Microsoft.Extensions.Options;
using System.Data;

namespace CRMService.Service.Entity
{
    public class CompanyService(IOptions<ApiEndpointOptions> endpoint, IOptions<OkdeskOptions> okdSettings,
        GetItemService _request, IUnitOfWork unitOfWork, PGSelect pGSelect, ILoggerFactory logger)
    {
        private readonly ILogger<CompanyService> _logger = logger.CreateLogger<CompanyService>();

        public async Task<Company?> GetCompanyFromCloudApi(int companyId)
        {
            string link = $"{endpoint.Value.OkdeskApi}/companies?api_token={okdSettings.Value.OkdeskApiToken}&id={companyId}";

            return await _request.GetItem<Company>(link);
        }

        private async IAsyncEnumerable<List<Company>> GetCompaniesFromCloudApiByCategory(IEnumerable<CompanyCategory> categories, long startIndex, long limit)
        {
            foreach (var category in categories)
            {
                string link = $"{endpoint.Value.OkdeskApi}/companies/list?api_token={okdSettings.Value.OkdeskApiToken}&category_ids[]={category.Id}";
                await foreach (List<Company> companies in _request.GetAllItems<Company>(link, startIndex, limit))
                {
                    foreach (var company in companies)
                    {
                        company.CategoryId = category.Id;
                        company.Category = null;
                    }

                    yield return companies;
                }
            }
        }

        private async IAsyncEnumerable<List<Company>?> GetCompaniesFromCloudDbByCategory(IEnumerable<CompanyCategory> categories, long startIndexCompany)
        {
            foreach (CompanyCategory category in categories)
            {
                while (true)
                {
                    string sqlCommand = string.Format(
                        "SELECT companies.sequential_id AS id, companies.name, companies.additional_name, companies.active " +
                        "FROM companies " +
                        "LEFT OUTER JOIN company_categories ON companies.category_id = company_categories.id " +
                        "WHERE companies.sequential_id > '{0}' ", startIndexCompany);

                    if (category.Code == "no_category")
                        sqlCommand += " AND company_categories.code IS NULL ";
                    else sqlCommand += $" AND company_categories.code = '{category.Code}' ";

                    sqlCommand += $" ORDER BY companies.sequential_id LIMIT {LimitConstants.LIMIT_FOR_RETRIEVING_ENTITIES_FROM_DB};";

                    DataSet ds = await pGSelect.Select(sqlCommand);
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

                    startIndexCompany = companies.Last().Id;
                    yield return companies;
                }
                startIndexCompany = 0;
            }
        }

        public async Task UpdateCompanyFromCloudApi(int companyId, CancellationToken ct)
        {
            Company? company = await GetCompanyFromCloudApi(companyId);

            if (company == null)
                return;

            if (!await CheckCompanyCategory(company, ct))
                return;

            await unitOfWork.Company.Upsert(company, ct);

            await unitOfWork.SaveAsync(ct);
        }

        public async Task UpdateCompaniesFromCloudApi(int startIndexCategory, long startIndexCompany, CancellationToken ct)
        {
            _logger.LogInformation("[Method:{MethodName}] Starting updating companies.", nameof(UpdateCompaniesFromCloudApi));

            IEnumerable<CompanyCategory> categories = await unitOfWork.CompanyCategory.GetItemsByPredicateAndSortById(predicate: c => c.Id >= startIndexCategory, asNoTracking: true, ct: ct);

            if (categories == null || !categories.Any())
                return;

            await foreach (List<Company> companies in GetCompaniesFromCloudApiByCategory(categories, startIndexCompany, LimitConstants.LIMIT_FOR_RETRIEVING_ENTITIES_FROM_API))
            {
                if (companies == null || companies.Count == 0)
                    continue;

                await unitOfWork.Company.Upsert(companies, ct);

                await unitOfWork.SaveAsync(ct);
            }

            _logger.LogInformation("[Method:{MethodName}] Companies update completed.", nameof(UpdateCompaniesFromCloudApi));
        }

        public async Task UpdateCompaniesFromCloudDb(int startIndexCategory, long startIndexCompany, CancellationToken ct)
        {
            _logger.LogInformation("[Method:{MethodName}] Starting updating companies.", nameof(UpdateCompaniesFromCloudDb));

            IEnumerable<CompanyCategory> categories = await unitOfWork.CompanyCategory.GetItemsByPredicateAndSortById(predicate: c => c.Id >= startIndexCategory, asNoTracking: true, ct: ct);

            if (categories == null || !categories.Any())
                return;

            await foreach (List<Company>? companies in GetCompaniesFromCloudDbByCategory(categories, startIndexCompany))
            {
                if (companies == null || companies.Count == 0)
                    continue;

                await unitOfWork.Company.Upsert(companies, ct);

                await unitOfWork.SaveAsync(ct);
            }

            _logger.LogInformation("[Method:{MethodName}] Companies update completed.", nameof(UpdateCompaniesFromCloudDb));
        }

        public async Task<bool> CheckCompanyCategory(Company company, CancellationToken ct)
        {
            if (company.Category == null)
            {
                company.CategoryId = 0;
                return true;
            }

            CompanyCategory? category = await unitOfWork.CompanyCategory.GetItemById(company.Category.Id, true, ct);
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
