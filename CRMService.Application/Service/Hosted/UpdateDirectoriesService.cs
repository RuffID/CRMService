using CRMService.Application.Abstractions.Service;
using Microsoft.Extensions.Logging;

namespace CRMService.Application.Service.Hosted
{
    public class UpdateDirectoriesService(
        IDirectoryUpdateOperations operations,
        ILoggerFactory logger)
    {
        private readonly ILogger<UpdateDirectoriesService> _logger = logger.CreateLogger<UpdateDirectoriesService>();

        public async Task RunUpdateDirectories(CancellationToken ct = default)
        {
            _logger.LogInformation("[Method:{MethodName}] Starting updating directories.", nameof(RunUpdateDirectories));

            await operations.UpdateKindsAsync(ct);
            await operations.UpdateKindParametersAsync(ct);
            await operations.UpdateKindParameterConnectionsAsync(ct);
            await operations.UpdateManufacturersAsync(ct);
            await operations.UpdateModelsAsync(ct);
            await operations.EnsureAnonymousCategoryAsync(ct);
            await operations.UpdateCategoriesAsync(ct);
            await operations.UpdateCompaniesAsync(ct);
            await operations.UpdateMaintenanceEntitiesAsync(ct);
            await operations.UpdateRolesAsync(ct);
            await operations.UpdateGroupsAsync(ct);
            await operations.UpdateEmployeesAsync(ct);
            await operations.UpdateEmployeeGroupConnectionsAsync(ct);
            await operations.UpdateEmployeeRoleConnectionsAsync(ct);
            await operations.UpdateIssuePrioritiesAsync(ct);
            await operations.UpdateIssueTypesAsync(ct);
            await operations.UpdateIssueStatusesAsync(ct);

            _logger.LogInformation("[Method:{MethodName}] Directories update completed.", nameof(RunUpdateDirectories));
        }
    }
}
