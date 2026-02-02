/*using CRMService.Interfaces.Repository;
using CRMService.Models.Authorization;
using CRMService.Models.Constants;
using CRMService.Models.Dto.Authorization;
using CRMService.Models.Dto.Mappers.Authorize;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRMService.Controllers.Authorization
{
    [Authorize]
    [ApiController]
    [Route("api/authorize/[controller]")]
    public class RoleController(IUnitOfWork unitOfWork) : Controller
    {
        [HttpGet("list"), Authorize(Roles = RolesConstants.ADMIN)]
        public async Task<IActionResult> GetRoles([FromQuery] int skip = 0, [FromQuery] int limit = LimitConstants.LIMIT_FOR_RETRIEVING_ENTITIES_FROM_DB, CancellationToken ct = default)
        {
            List<CrmRole> roles = await unitOfWork.CrmRole.GetItemsByPredicate(skip: skip, take: limit, asNoTracking: true, ct: ct);

            return Ok(roles.ToDto());
        }

        [HttpGet, Authorize(Roles = RolesConstants.ADMIN)]
        public async Task<IActionResult> GetRole([FromQuery] Guid id, CancellationToken ct)
        {
            CrmRole? role = await unitOfWork.CrmRole.GetItemById(id, asNoTracking: true, ct);

            if (role == null)
                return NotFound("Role not found.");

            return Ok(role.ToDto());
        }

        [HttpPost, Authorize(Roles = RolesConstants.ADMIN)]
        public async Task<IActionResult> CreateRole([FromBody] CrmRoleDto roleCreate, CancellationToken ct)
        {
            CrmRole? existRole = await unitOfWork.CrmRole.GetItemById(roleCreate.Id, asNoTracking: true, ct);

            if (existRole != null)
                return Conflict($"Role {roleCreate.Name} is already exists.");

            unitOfWork.CrmRole.Create(roleCreate.ToEntity());
            await unitOfWork.SaveAsync(ct);

            return NoContent();
        }
    }
}
*/