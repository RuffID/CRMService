using CRMService.API;
using CRMService.Interfaces.Repository;
using CRMService.Models.ConfigClass;
using CRMService.Models.Entity;
using Microsoft.Extensions.Options;

namespace CRMService.Service.Entity
{
    public class RoleService(IOptions<ApiEndpoint> endpoint, IOptions<OkdeskSettings> okdeskSettings, GetItemService request, IUnitOfWorkEntities unitOfWork, ILoggerFactory logger)
    {
        private readonly ILogger<RoleService> _logger = logger.CreateLogger<RoleService>();

        private async Task<List<Role>?> GetRolesFromCloudApi()
        {
            string link = $"{endpoint.Value.OkdeskApi}/employees/roles?api_token={okdeskSettings.Value.ApiToken}";

            return await request.GetRangeOfItems<Role>(link);
        }

        public async Task UpdateRolesFromCloudApi()
        {
            _logger.LogInformation("[Method:{MethodName}] Starting updating employee roles.", nameof(UpdateRolesFromCloudApi));

            List<Role>? roles = await GetRolesFromCloudApi();

            if (roles == null || roles.Count == 0)
                return;

            await unitOfWork.Role.CreateOrUpdate(roles);

            await unitOfWork.SaveAsync();

            _logger.LogInformation("[Method:{MethodName}] Employee roles update completed.", nameof(UpdateRolesFromCloudApi));
        }
    }
}