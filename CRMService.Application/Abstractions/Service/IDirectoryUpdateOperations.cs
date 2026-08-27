namespace CRMService.Application.Abstractions.Service
{
    public interface IDirectoryUpdateOperations
    {
        Task UpdateKindsAsync(CancellationToken ct);
        Task UpdateKindParametersAsync(CancellationToken ct);
        Task UpdateKindParameterConnectionsAsync(CancellationToken ct);
        Task UpdateManufacturersAsync(CancellationToken ct);
        Task UpdateModelsAsync(CancellationToken ct);
        Task EnsureAnonymousCategoryAsync(CancellationToken ct);
        Task UpdateCategoriesAsync(CancellationToken ct);
        Task UpdateCompaniesAsync(CancellationToken ct);
        Task UpdateMaintenanceEntitiesAsync(CancellationToken ct);
        Task UpdateRolesAsync(CancellationToken ct);
        Task UpdateGroupsAsync(CancellationToken ct);
        Task UpdateEmployeesAsync(CancellationToken ct);
        Task UpdateEmployeeGroupConnectionsAsync(CancellationToken ct);
        Task UpdateEmployeeRoleConnectionsAsync(CancellationToken ct);
        Task UpdateIssuePrioritiesAsync(CancellationToken ct);
        Task UpdateIssueTypesAsync(CancellationToken ct);
        Task UpdateIssueStatusesAsync(CancellationToken ct);
    }
}
