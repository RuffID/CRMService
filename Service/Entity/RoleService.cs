using CRMService.API;
using CRMService.Interfaces.Repository;
using CRMService.Models.ConfigClass;
using CRMService.Models.Entity;
using Microsoft.Extensions.Options;

namespace CRMService.Service.Entity
{
    public class RoleService(IOptions<ApiEndpointOptions> endpoint, IOptions<OkdeskOptions> okdeskSettings, GetItemService request, IUnitOfWork unitOfWork, ILoggerFactory logger)
    {
        private readonly ILogger<RoleService> _logger = logger.CreateLogger<RoleService>();

        private async Task<List<OkdeskRole>?> GetRolesFromCloudApi()
        {
            string link = $"{endpoint.Value.OkdeskApi}/employees/roles?api_token={okdeskSettings.Value.ApiToken}";

            return await request.GetRangeOfItems<OkdeskRole>(link);
        }

        public async Task UpdateRolesFromCloudApi(CancellationToken ct)
        {
            _logger.LogInformation("[Method:{MethodName}] Starting updating employee roles.", nameof(UpdateRolesFromCloudApi));

            List<OkdeskRole>? roles = await GetRolesFromCloudApi();

            if (roles == null || roles.Count == 0)
                return;

            await unitOfWork.OkdeskRole.Upsert(roles, ct);

            await unitOfWork.SaveAsync(ct);

            _logger.LogInformation("[Method:{MethodName}] Employee roles update completed.", nameof(UpdateRolesFromCloudApi));
        }
    }
}