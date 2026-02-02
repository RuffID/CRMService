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
        ILoggerFactory logger)
    {
        private readonly ILogger<UpdateDirectoriesService> _logger = logger.CreateLogger<UpdateDirectoriesService>();

        public async Task RunUpdateDirectories(CancellationToken ct = default)
        {
            _logger.LogInformation("[Method:{MethodName}] Starting updating directories.", nameof(RunUpdateDirectories));

            await kind.UpdateKindsFromCloudApi(startIndex: 0, limit: LimitConstants.LIMIT_FOR_RETRIEVING_ENTITIES_FROM_API, ct);

            await kindParameter.UpdateKindParametersFromCloudDb(ct);

            await kindParam.UpsertConnectionsFromCloudDb(ct);

            await manufacturer.UpdateManufacturersFromCloudApi(startIndex: 0, limit: LimitConstants.LIMIT_FOR_RETRIEVING_ENTITIES_FROM_API, ct);

            await model.UpdateModelsFromCloudApi(startIndex: 0, limit: LimitConstants.LIMIT_FOR_RETRIEVING_ENTITIES_FROM_API, ct);

            await category.CheckAnonymousCategory(ct);

            await category.UpdateCategoriesFromCloudDb(ct);

            await company.UpdateCompaniesFromCloudApi(startIndexCategory: 0, startIndexCompany: 0, ct);

            await maintenance.UpdateMaintenanceEntitiesFromCloudApi(startIndex: 0, limit: LimitConstants.LIMIT_FOR_RETRIEVING_ENTITIES_FROM_API, ct);

            await role.UpdateRolesFromCloudApi(ct);

            await group.UpdateGroupsFromCloudApi(ct);

            await employee.UpdateEmployeesFromCloudApi(startIndex: 0, limit: LimitConstants.LIMIT_FOR_RETRIEVING_ENTITIES_FROM_API, ct);

            await group.UpsertEmployeeGroupConnectionsFromApi(ct);

            await role.UpsertEmployeeRoleConnectionsFromApi(ct);

            await priority.UpdateIssuePrioritiesFromCloudApi(ct);

            await type.UpdateIssueTypesFromCloudApi(ct);

            await status.UpdateIssueStatusesFromCloudApi(ct);

            _logger.LogInformation("[Method:{MethodName}] Directories update completed.", nameof(RunUpdateDirectories));
        }
    }
}