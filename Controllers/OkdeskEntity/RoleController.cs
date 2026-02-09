using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CRMService.Service.OkdeskEntity;
using CRMService.Models.OkdeskEntity;
using CRMService.Models.Constants;
using CRMService.Models.Dto.Mappers.OkdeskEntity;
using CRMService.Abstractions.Database.Repository;

namespace CRMService.Controllers.OkdeskEntity
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class RoleController(IUnitOfWork unitOfWork, RoleService roleService) : Controller
    {
        [HttpGet("list")]
        public async Task<IActionResult> GetRoles(CancellationToken ct = default)
        {
            List<OkdeskRole> roles = await unitOfWork.OkdeskRole.GetItemsByPredicateAsync(asNoTracking: true, ct: ct);

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
