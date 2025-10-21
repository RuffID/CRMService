using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CRMService.Service.OkdeskEntity;
using CRMService.Interfaces.Repository;
using CRMService.Models.OkdeskEntity;
using CRMService.Models.Constants;
using CRMService.Models.Dto.Mappers.OkdeskEntity;

namespace CRMService.Controllers.OkdeskEntity
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class RoleController(IUnitOfWork unitOfWork, RoleService roleService) : Controller
    {
        [HttpGet("list")]
        public async Task<IActionResult> GetRoles([FromQuery] int startIndex = 0, CancellationToken ct = default)
        {
            List<OkdeskRole> roles = await unitOfWork.OkdeskRole.GetItemsByPredicateAndSortById(predicate: r => r.Id >= startIndex, take: LimitConstants.LIMIT_FOR_RETRIEVING_ENTITIES_FROM_DB, asNoTracking: true, ct: ct);

            return Ok(roles.ToDto());
        }

        [HttpPut("update_from_cloud_api"), Authorize(Roles = RolesConstants.ADMIN)]
        public async Task<IActionResult> UpdateRolesFromCloudApi(CancellationToken ct)
        {
            await roleService.UpdateRolesFromCloudApi(ct);

            return NoContent();
        }

        [HttpPut("update_connections_with_employees_from_cloud_api"), Authorize(Roles = RolesConstants.ADMIN)]
        public async Task<IActionResult> UpdateEmployeeRoleConnectionsFromCloudApi(CancellationToken ct)
        {
            await roleService.UpsertEmployeeRoleConnectionsFromApi(ct);

            return NoContent();
        }
    }
}
