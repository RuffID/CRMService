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
    [Route("api/authorize/[controller]")]
    public class RoleController(IMapper mapper, IUnitOfWorkAuthorization unitOfWork) : Controller
    {
        [HttpGet("list"), Authorize(Roles = RolesDefinition.ADMIN)]
        public async Task<IActionResult> GetRoles([FromQuery] int startIndex = 0, [FromQuery] int endIndex = 100)
        {
            IEnumerable<RoleDto> roles = mapper.Map<IEnumerable<RoleDto>>(await unitOfWork.Role.GetItems(new Range(startIndex, endIndex)));

            if (roles == null)
                return NotFound("Roles not found.");

            return Ok(roles);
        }

        [HttpGet, Authorize(Roles = RolesDefinition.ADMIN)]
        public async Task<IActionResult> GetRole([FromQuery] Guid id)
        {
            RoleDto role = mapper.Map<RoleDto>(await unitOfWork.Role.GetItem(new() { Id = id }, false));

            if (role == null)
                return NotFound("Role not found.");

            return Ok(role);
        }

        [HttpPost, Authorize(Roles = RolesDefinition.ADMIN)]
        public async Task<IActionResult> CreateRole([FromBody] RoleDto role)
        {
            if (await unitOfWork.Role.GetItem(mapper.Map<Role>(role), false) != null)
                return BadRequest($"Role {role.Name} is already exists.");

            role.Id = Guid.NewGuid();
            unitOfWork.Role.Create(mapper.Map<Role>(role));
            await unitOfWork.SaveAsync();

            return NoContent();
        }
    }
}
