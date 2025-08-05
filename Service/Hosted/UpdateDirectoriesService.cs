using CRMService.Models.ConfigClass;
using CRMService.Service.Entity;
using Microsoft.Extensions.Options;

namespace CRMService.Service.Hosted
{
    public class UpdateDirectoriesService(
        KindService kind,
        KindParameterService kindParameter,
        KindParamService kindParam,
        ManufacturerService manufacturer,
        ModelService model,
        CompanyCategoryService category,
        CompanyService company,
        MaintenanceEntityService maintenance,
        RoleService role,
        EmployeeService employee,
        GroupService group,
        IssuePriorityService priority,
        IssueTypeService type,
        IssueStatusService status,
        IssueService issue,
        TimeEntryService time,
        EquipmentService equipment,
        IOptions<OkdeskSettings> okdeskSettings,
        IOptions<DatabaseSettings> dbSettings,
        ILoggerFactory logger)
    {
        private readonly ILogger<UpdateDirectoriesService> _logger = logger.CreateLogger<UpdateDirectoriesService>();

        public async Task RunUpdateDirectories(DateTime? dateFrom = null)
        {
            DateTime now = DateTime.Now;
            if (!dateFrom.HasValue)
                dateFrom = new(now.Year, now.Month, now.Day - 1, hour: 0, minute: 0, second: 0);

            DateTime dateTo = now.AddHours(-4); // Смещение на 4 часа назад, чтобы не затирались более новые данные устаревшими

            _logger.LogInformation("[Method:{MethodName}] Starting updating directories. Date from: {DateFrom}, date to: {DateTo}", nameof(RunUpdateDirectories), dateFrom.ToString(), dateTo.ToString());

            await kind.UpdateKindsFromCloudApi(startIndex: 0, limit: okdeskSettings.Value.LimitForRetrievingEntitiesFromApi);

            await kindParameter.UpdateKindParametersFromCloudDb();

            await kindParam.UpdateConnectionsFromCloudDb();

            await manufacturer.UpdateManufacturersFromCloudApi(startIndex: 0, limit: okdeskSettings.Value.LimitForRetrievingEntitiesFromApi);

            await model.UpdateModelsFromCloudApi(startIndex: 0, limit: okdeskSettings.Value.LimitForRetrievingEntitiesFromApi);

            await category.CheckAnonymousCategory();

            await category.UpdateCategoriesFromCloudDb();

            await company.UpdateCompaniesFromCloudDb(startIndexCategory: 0, startIndexCompany: 0);

            await maintenance.UpdateMaintenanceEntitiesFromCloudDb();

            await role.UpdateRolesFromCloudApi();

            await employee.UpdateEmployeesFromCloudApi(startIndex: 0, limit: okdeskSettings.Value.LimitForRetrievingEntitiesFromApi);

            await group.UpdateGroupsFromCloudApi();

            await priority.UpdateIssuePrioritiesFromCloudDb();

            await type.UpdateIssueTypesFromCloudDb();

            await status.UpdateIssueStatusesFromCloudDb();

            await issue.UpdateIssuesFromCloudDb(dateFrom.Value, dateTo, startIndex: 0, limit: dbSettings.Value.LimitForRetrievingEntitiesFromDb);

            await time.UpdateTimeEntriesFromCloudDb(dateFrom.Value, dateTo);

            await equipment.UpdateEquipmentsFromCloudDb(startIndex: 0, limit: dbSettings.Value.LimitForRetrievingEntitiesFromDb);

            _logger.LogInformation("[Method:{MethodName}] Directories update completed...", nameof(RunUpdateDirectories));
        }
    }
}
