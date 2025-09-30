using CRMService.API;
using CRMService.Interfaces.Repository;
using CRMService.Models.ConfigClass;
using CRMService.Models.OkdeskEntity;
using Microsoft.Extensions.Options;

namespace CRMService.Service.OkdeskEntity
{
    public class RoleService(IOptions<ApiEndpointOptions> endpoint, IOptions<OkdeskOptions> okdeskSettings, GetOkdeskEntityService request, IUnitOfWork unitOfWork)
    {
        private async Task<List<OkdeskRole>> GetRolesFromCloudApi()
        {
            string link = $"{endpoint.Value.OkdeskApi}/employees/roles?api_token={okdeskSettings.Value.OkdeskApiToken}";

            return await request.GetRangeOfItems<OkdeskRole>(link);
        }

        public async Task UpdateRolesFromCloudApi(CancellationToken ct)
        {
            List<OkdeskRole> roles = await GetRolesFromCloudApi();

            if (roles.Count == 0)
                return;

            await unitOfWork.OkdeskRole.Upsert(roles, ct);

            await unitOfWork.SaveAsync(ct);
        }
    }
}