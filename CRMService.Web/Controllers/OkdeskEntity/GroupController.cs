using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CRMService.Application.Service.OkdeskEntity;
using CRMService.Domain.Models.Constants;
using CRMService.Web.Service.BackgroundServices;

namespace CRMService.Web.Controllers.OkdeskEntity
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class GroupController(GroupService service, BackgroundUpdateService backgroundUpdateService) : Controller
    {
        [HttpGet("list")]
        public async Task<IActionResult> GetGroups(CancellationToken ct = default)
        {
            return JsonResultMapper.ToJsonResult(await service.GetGroups(ct));
        }

        [HttpPut("update_from_cloud_api"), Authorize(Roles = RolesConstants.ADMIN)]
        public IActionResult UpdateGroupsFromCloudApi(CancellationToken ct)
        {
            bool started = backgroundUpdateService.TryStart(
                nameof(UpdateGroupsFromCloudApi),
                (provider, token) => provider.GetRequiredService<GroupService>().UpdateGroupsFromCloudApi(token));

            return this.ToBackgroundUpdateResponse(started);
        }

        [HttpPut("update_from_cloud_db"), Authorize(Roles = RolesConstants.ADMIN)]
        public IActionResult UpdateGroupsFromCloudDb(CancellationToken ct)
        {
            bool started = backgroundUpdateService.TryStart(
                nameof(UpdateGroupsFromCloudDb),
                (provider, token) => provider.GetRequiredService<GroupService>().UpdateGroupsFromCloudDb(token));

            return this.ToBackgroundUpdateResponse(started);
        }

        [HttpPut("update_connections_with_employees_from_cloud_api"), Authorize(Roles = RolesConstants.ADMIN)]
        public IActionResult UpdateGroupConnectionsWithEmployeeFromCloudApi(CancellationToken ct)
        {
            bool started = backgroundUpdateService.TryStart(
                nameof(UpdateGroupConnectionsWithEmployeeFromCloudApi),
                (provider, token) => provider.GetRequiredService<GroupService>().UpsertEmployeeGroupConnectionsFromApi(token));

            return this.ToBackgroundUpdateResponse(started);
        }
    }
}
