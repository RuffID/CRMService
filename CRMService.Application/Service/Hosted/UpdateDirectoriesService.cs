using CRMService.Application.Service.OkdeskEntity;

namespace CRMService.Application.Service.Hosted
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

            await kind.UpdateKindsFromCloudApi(ct);

            await kindParameter.UpdateKindParametersFromCloudDb(ct);

            await kindParam.UpsertConnectionsFromCloudDb(ct);

            await manufacturer.UpdateManufacturersFromCloudApi(ct);

            await model.UpdateModelsFromCloudApi(ct);

            await category.CheckAnonymousCategory(ct);

            await category.UpdateCategoriesFromCloudDb(ct);

            await company.UpdateCompaniesFromCloudApi(ct);

            await maintenance.UpdateMaintenanceEntitiesFromCloudApi(ct);

            await role.UpdateRolesFromCloudApi(ct);

            await group.UpdateGroupsFromCloudApi(ct);

            await employee.UpdateEmployeesFromCloudApi(ct);

            await group.UpsertEmployeeGroupConnectionsFromApi(ct);

            await role.UpsertEmployeeRoleConnectionsFromApi(ct);

            await priority.UpdateIssuePrioritiesFromCloudApi(ct);

            await type.UpdateIssueTypesFromCloudApi(ct);

            await status.UpdateIssueStatusesFromCloudApi(ct);

            _logger.LogInformation("[Method:{MethodName}] Directories update completed.", nameof(RunUpdateDirectories));
        }
    }
}