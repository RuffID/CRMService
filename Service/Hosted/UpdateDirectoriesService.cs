using CRMService.Models.Constants;
using CRMService.Service.OkdeskEntity;

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
        ILoggerFactory logger)
    {
        private readonly ILogger<UpdateDirectoriesService> _logger = logger.CreateLogger<UpdateDirectoriesService>();

        public async Task RunUpdateDirectories(DateTime? dateFrom = null, DateTime? dateTo = null, CancellationToken ct = default)
        {
            DateTime now = DateTime.Now;
            if (!dateFrom.HasValue)
                dateFrom = new(now.Year, now.Month, now.Day, hour: 0, minute: 0, second: 0);

            if (!dateTo.HasValue)
                dateTo = now;

            _logger.LogInformation("[Method:{MethodName}] Starting updating directories. Date from: {DateFrom}, date to: {DateTo}", nameof(RunUpdateDirectories), dateFrom.ToString(), dateTo.ToString());

            await kind.UpdateKindsFromCloudApi(startIndex: 0, limit: LimitConstants.LIMIT_FOR_RETRIEVING_ENTITIES_FROM_API, ct);

            await kindParameter.UpdateKindParametersFromCloudDb(ct);

            await kindParam.UpdateConnectionsFromCloudDb(ct);

            await manufacturer.UpdateManufacturersFromCloudApi(startIndex: 0, limit: LimitConstants.LIMIT_FOR_RETRIEVING_ENTITIES_FROM_API, ct);

            await model.UpdateModelsFromCloudApi(startIndex: 0, limit: LimitConstants.LIMIT_FOR_RETRIEVING_ENTITIES_FROM_API, ct);

            await category.CheckAnonymousCategory(ct);

            await category.UpdateCategoriesFromCloudDb(ct);

            await company.UpdateCompaniesFromCloudApi(startIndexCategory: 0, startIndexCompany: 0, ct);

            await maintenance.UpdateMaintenanceEntitiesFromCloudApi(startIndex: 0, limit: LimitConstants.LIMIT_FOR_RETRIEVING_ENTITIES_FROM_API, ct);

            await role.UpdateRolesFromCloudApi(ct);

            await employee.UpdateEmployeesFromCloudApi(startIndex: 0, limit: LimitConstants.LIMIT_FOR_RETRIEVING_ENTITIES_FROM_API, ct);

            await group.UpdateGroupsFromCloudApi(ct);

            await priority.UpdateIssuePrioritiesFromCloudDb(ct);

            await type.UpdateIssueTypesFromCloudDb(ct);

            await status.UpdateIssueStatusesFromCloudDb(ct);

            await issue.UpdateIssuesFromCloudApi(dateFrom.Value, dateTo.Value, startIndex: 0, limit: LimitConstants.LIMIT_FOR_RETRIEVING_ENTITIES_FROM_DB, ct: ct);

            await time.UpdateTimeEntriesFromCloudDb(dateFrom: dateFrom.Value, dateTo: dateTo.Value, startIndex: 0, limit: LimitConstants.LIMIT_FOR_RETRIEVING_ENTITIES_FROM_DB, ct: ct);

            // Пока нет нужны в оборудовании в БД
            //await equipment.UpdateEquipmentsFromCloudApi(startIndex: 0, limit: dbSettings.Value.LimitForRetrievingEntitiesFromDb);

            _logger.LogInformation("[Method:{MethodName}] Directories update completed...", nameof(RunUpdateDirectories));
        }
    }
}