using CRMService.Application.Abstractions.Service;
using CRMService.Application.Service.OkdeskEntity;

namespace CRMService.Application.Service.Hosted
{
    public class DirectoryUpdateOperations(
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
        IssueStatusService status) : IDirectoryUpdateOperations
    {
        public Task UpdateKindsAsync(CancellationToken ct) => kind.UpdateKindsFromCloudApi(ct);
        public Task UpdateKindParametersAsync(CancellationToken ct) => kindParameter.UpdateKindParametersFromCloudDb(ct);
        public Task UpdateKindParameterConnectionsAsync(CancellationToken ct) => kindParam.UpsertConnectionsFromCloudDb(ct);
        public Task UpdateManufacturersAsync(CancellationToken ct) => manufacturer.UpdateManufacturersFromCloudApi(ct);
        public Task UpdateModelsAsync(CancellationToken ct) => model.UpdateModelsFromCloudApi(ct);
        public Task EnsureAnonymousCategoryAsync(CancellationToken ct) => category.CheckAnonymousCategory(ct);
        public Task UpdateCategoriesAsync(CancellationToken ct) => category.UpdateCategoriesFromCloudDb(ct);
        public Task UpdateCompaniesAsync(CancellationToken ct) => company.UpdateCompaniesFromCloudApi(ct);
        public Task UpdateMaintenanceEntitiesAsync(CancellationToken ct) => maintenance.UpdateMaintenanceEntitiesFromCloudApi(ct);
        public Task UpdateRolesAsync(CancellationToken ct) => role.UpdateRolesFromCloudApi(ct);
        public Task UpdateGroupsAsync(CancellationToken ct) => group.UpdateGroupsFromCloudApi(ct);
        public Task UpdateEmployeesAsync(CancellationToken ct) => employee.UpdateEmployeesFromCloudApi(ct);
        public Task UpdateEmployeeGroupConnectionsAsync(CancellationToken ct) => group.UpsertEmployeeGroupConnectionsFromApi(ct);
        public Task UpdateEmployeeRoleConnectionsAsync(CancellationToken ct) => role.UpsertEmployeeRoleConnectionsFromApi(ct);
        public Task UpdateIssuePrioritiesAsync(CancellationToken ct) => priority.UpdateIssuePrioritiesFromCloudApi(ct);
        public Task UpdateIssueTypesAsync(CancellationToken ct) => type.UpdateIssueTypesFromCloudDb(ct);
        public Task UpdateIssueStatusesAsync(CancellationToken ct) => status.UpdateIssueStatusesFromCloudApi(ct);
    }
}
