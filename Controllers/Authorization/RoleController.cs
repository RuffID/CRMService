using CRMService.Interfaces.Repository;
using CRMService.Models.Authorization;
using CRMService.Models.ConfigClass;
using CRMService.Models.Dto.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace CRMService.Controllers.Authorization
{
    [Authorize]
    [ApiController]
    [Route("api/authorize/[controller]")]
    public class RoleController(IUnitOfWork unitOfWork) : Controller
    {
        [HttpGet("list"), Authorize(Roles = RolesDefinition.ADMIN)]
        public async Task<IActionResult> GetRoles([FromQuery] int skip = 0, [FromQuery] int limit = LimitConstants.LIMIT_FOR_RETRIEVING_ENTITIES_FROM_DB, CancellationToken ct = default)
        {
            List<CrmRole> roles = await unitOfWork.CrmRole.GetItemsByPredicate(skip: skip, take: limit, asNoTracking: true, ct: ct);

            List<RoleDto> roleDtos = roles.Select(r => new RoleDto
            {
                Id = r.Id,
                Name = r.Name,
            }).ToList();

            return Ok(roleDtos);
        }

        [HttpGet, Authorize(Roles = RolesDefinition.ADMIN)]
        public async Task<IActionResult> GetRole([FromQuery] Guid id, CancellationToken ct)
        {
            CrmRole? role = await unitOfWork.CrmRole.GetItemById(id, asNoTracking: true, ct);

            if (role == null)
                return NotFound("Role not found.");

            RoleDto dto = new RoleDto
            {
                Id = role.Id,
                Name = role.Name,
            };

            return Ok(dto);
        }

        [HttpPost, Authorize(Roles = RolesDefinition.ADMIN)]
        public async Task<IActionResult> CreateRole([FromBody] RoleDto role, CancellationToken ct)
        {
            CrmRole? existRole = await unitOfWork.CrmRole.GetItemById(role.Id, asNoTracking: true, ct);

            if (existRole != null)
                return Conflict($"Role {role.Name} is already exists.");

            CrmRole roleCreate = new CrmRole
            {
                Id = role.Id,
                Name = role.Name,
            };

            unitOfWork.CrmRole.Create(roleCreate);
            await unitOfWork.SaveAsync(ct);

            return NoContent();
        }
    }
}
