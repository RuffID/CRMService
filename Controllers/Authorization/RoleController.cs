using AutoMapper;
using CRMService.Interfaces.Repository;
using CRMService.Models.Authorization;
using CRMService.Models.ConfigClass;
using CRMService.Models.Dto.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRMService.Controllers.Authorization
{
    [Authorize]
    [ApiController]
    [Route("api/authorize/[controller]")]
    public class RoleController(IMapper mapper, IUnitOfWorkAuthorization unitOfWork) : Controller
    {
        [HttpGet("list"), Authorize(Roles = RolesDefinition.ADMIN)]
        public async Task<IActionResult> GetRoles([FromQuery] int skip = 0, [FromQuery] int limit = LimitConstants.LIMIT_FOR_RETRIEVING_ENTITIES_FROM_DB, CancellationToken ct = default)
        {
            List<CrmRole> roles = await unitOfWork.Role.GetItemsByPredicate(skip: skip, take: limit, asNoTracking: true, ct: ct);

            return Ok(mapper.Map<IEnumerable<RoleDto>>(roles));
        }

        [HttpGet, Authorize(Roles = RolesDefinition.ADMIN)]
        public async Task<IActionResult> GetRole([FromQuery] Guid id)
        {
            CrmRole? role = await unitOfWork.Role.GetItemById(id, asNoTracking: true);

            if (role == null)
                return NotFound("Role not found.");

            return Ok(mapper.Map<RoleDto>(role));
        }

        [HttpPost, Authorize(Roles = RolesDefinition.ADMIN)]
        public async Task<IActionResult> CreateRole([FromBody] RoleDto role)
        {
            CrmRole? existRole = await unitOfWork.Role.GetItemById(role.Id, asNoTracking: true);

            if (existRole != null)
                return Conflict($"Role {role.Name} is already exists.");

            unitOfWork.Role.Create(mapper.Map<CrmRole>(role));
            await unitOfWork.SaveAsync();

            return NoContent();
        }
    }
}
