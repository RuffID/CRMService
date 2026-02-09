using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CRMService.Service.OkdeskEntity;
using CRMService.Models.Constants;

namespace CRMService.Controllers.OkdeskEntity
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class GroupController(GroupService service) : Controller
    {
        [HttpGet("list")]
        public async Task<IActionResult> GetGroups(CancellationToken ct = default)
        {
            return Ok((await service.GetGroups(ct)).Data);
        }

        [HttpPut("update_from_cloud_api"), Authorize(Roles = RolesConstants.ADMIN)]
        public async Task<IActionResult> UpdateGroupsFromCloudApi(CancellationToken ct)
        {
            await service.UpdateGroupsFromCloudApi(ct);

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