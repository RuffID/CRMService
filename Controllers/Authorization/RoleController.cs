using AutoMapper;
using CRMService.Dto.Authorization;
using CRMService.Interfaces.Repository;
using CRMService.Models.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRMService.Controllers.Authorization
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class RoleController(IMapper mapper, IUnitOfWorkAuthorization unitOfWork) : Controller
    {
        [HttpGet("list"), Authorize(Roles = RolesDefinition.ADMIN)]
        public async Task<IActionResult> GetRoles([FromQuery] int startIndex = 0, [FromQuery] int endIndex = 100)
        {
            var roles = mapper.Map<IEnumerable<RoleDto>>(await unitOfWork.Role.GetAllItem(new Range(startIndex, endIndex)));

            if (roles == null)
                return NotFound("Roles not found.");

            return Ok(roles);
        }

        [HttpGet, Authorize(Roles = RolesDefinition.ADMIN)]
        public async Task<IActionResult> GetRole([FromQuery] Guid id)
        {
            Role newRole = new() { Id = id };

            var role = mapper.Map<RoleDto>(await unitOfWork.Role.GetItem(newRole));

            if (role == null)
                return NotFound("Role not found.");

            return Ok(role);
        }

        [HttpPost, Authorize(Roles = RolesDefinition.ADMIN)]
        public async Task<IActionResult> CreateRole([FromBody] RoleDto role)
        {
            if (await unitOfWork.Role.GetItem(mapper.Map<Role>(role)) != null)
                return BadRequest($"Role {role.Name} is already exists.");

            role.Id = Guid.NewGuid();
            unitOfWork.Role.Create(mapper.Map<Role>(role));
            await unitOfWork.SaveAsync();

            return NoContent();
        }
    }
}
