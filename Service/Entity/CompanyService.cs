using CRMService.API;
using CRMService.DataBase.Postgresql;
using CRMService.Interfaces.Repository;
using CRMService.Models.ConfigClass;
using CRMService.Models.Entity;
using CRMService.Service.Sync;
using Microsoft.Extensions.Options;
using System.Data;

namespace CRMService.Service.Entity
{
    public class CompanyService(IOptions<ApiEndpoint> endpoint, EntitySyncService sync, IOptions<OkdeskSettings> okdSettings, IOptions<DatabaseSettings> dbSettings, GetItemService _request, IUnitOfWorkEntities unitOfWork, PGSelect pGSelect, ILoggerFactory logger)
    {
        private readonly ILogger<CompanyService> _logger = logger.CreateLogger<CompanyService>();

        public async Task<Company?> GetCompanyFromCloudApi(int companyId)
        {
            string link = $"{endpoint.Value.OkdeskApi}/companies?api_token={okdSettings.Value.ApiToken}&id={companyId}";

            return await _request.GetItem<Company>(link);
        }

        private async IAsyncEnumerable<List<Company>> GetCompaniesFromCloudApiByCategory(IEnumerable<CompanyCategory> categories, long startIndex, long limit)
        {
            foreach (var category in categories)
            {
                string link = $"{endpoint.Value.OkdeskApi}/companies/list?api_token={okdSettings.Value.ApiToken}&category_ids[]={category.Id}";
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

        private async IAsyncEnumerable<List<Company>?> GetCompaniesFromCloudDbByCategory(IEnumerable<CompanyCategory> categories, long startIndexCompany, int limit)
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

                    sqlCommand += $" ORDER BY companies.sequential_id LIMIT {limit};";

                    DataSet ds = await pGSelect.Select(sqlCommand);
                    DataTable? companyTable = ds.Tables["Table"];
                    if (companyTable == null)
                        break;

                    List<Company> companies = companyTable.AsEnumerable().
                        Select(company => new Company
                        {
                            Id = company.Field<int>("id"),
                            Name = company.Field<string>("name"),
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

        public async Task UpdateCompanyFromCloudApi(int companyId)
        {
            Company? company = await GetCompanyFromCloudApi(companyId);

            if (company == null)
                return;

            await sync.RunExclusive(async () =>
            {
                if (!await CheckCompanyCategory(company))
                    return;

                await unitOfWork.Company.CreateOrUpdate([company]);

                await unitOfWork.SaveAsync();
            });
        }

        public async Task UpdateCompaniesFromCloudApi(int startIndexCategory, long startIndexCompany)
        {
            IEnumerable<CompanyCategory>? categories = await unitOfWork.CompanyCategory.GetItems(startIndexCategory, dbSettings.Value.LimitForRetrievingEntitiesFromDb);

            if (categories == null || !categories.Any())
                return;

            await sync.RunExclusive(async () =>
            {
                await foreach (List<Company> companies in GetCompaniesFromCloudApiByCategory(categories, startIndexCompany, okdSettings.Value.LimitForRetrievingEntitiesFromApi))
                {
                    if (companies == null || companies.Count == 0)
                        continue;

                    await unitOfWork.Company.CreateOrUpdate(companies);

                    await unitOfWork.SaveAsync();
                }
            });
        }

        public async Task UpdateCompaniesFromCloudDb(int startIndexCategory, long startIndexCompany)
        {
            _logger.LogInformation("[Method:{MethodName}] Starting updating companies.", nameof(UpdateCompaniesFromCloudDb));

            IEnumerable<CompanyCategory>? categories = await unitOfWork.CompanyCategory.GetItems(startIndexCategory, dbSettings.Value.LimitForRetrievingEntitiesFromDb);

            if (categories == null || !categories.Any())
                return;

            await sync.RunExclusive(async () =>
            {
                await foreach (List<Company>? companies in GetCompaniesFromCloudDbByCategory(categories, startIndexCompany, dbSettings.Value.LimitForRetrievingEntitiesFromDb))
                {
                    if (companies == null || companies.Count == 0)
                        continue;

                    await unitOfWork.Company.CreateOrUpdate(companies);

                    await unitOfWork.SaveAsync();
                }
            });

            _logger.LogInformation("[Method:{MethodName}] Companies update completed.", nameof(UpdateCompaniesFromCloudDb));
        }

        public async Task<bool> CheckCompanyCategory(Company company)
        {
            if (company.Category == null)
            {
                company.CategoryId = 0;
                return true;
            }

            CompanyCategory? category = await unitOfWork.CompanyCategory.GetItem(company.Category);
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
