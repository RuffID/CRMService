using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CRMService.Application.Service.OkdeskEntity;
using CRMService.Domain.Models.Constants;

namespace CRMService.Web.Controllers.OkdeskEntity
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class GroupController(GroupService service) : Controller
    {
        [HttpGet("list")]
        public async Task<IActionResult> GetGroups(CancellationToken ct = default)
        {
            return JsonResultMapper.ToJsonResult(await service.GetGroups(ct));
        }

        [HttpPut("update_from_cloud_api"), Authorize(Roles = RolesConstants.ADMIN)]
        public async Task<IActionResult> UpdateGroupsFromCloudApi(CancellationToken ct)
        {
            await service.UpdateGroupsFromCloudApi(ct);

            return NoContent();
        }

        [HttpPut("update_from_cloud_db"), Authorize(Roles = RolesConstants.ADMIN)]
        public async Task<IActionResult> UpdateGroupsFromCloudDb(CancellationToken ct)
        {
            await service.UpdateGroupsFromCloudDb(ct);

            return NoContent();
        }

        [HttpPut("update_connections_with_employees_from_cloud_api"), Authorize(Roles = RolesConstants.ADMIN)]
        public async Task<IActionResult> UpdateGroupConnectionsWithEmployeeFromCloudApi(CancellationToken ct)
        {
            await service.UpsertEmployeeGroupConnectionsFromApi(ct);

            return NoContent();
        }
    }
}