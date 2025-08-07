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
    public class UserRoleController(IUnitOfWorkAuthorization unitOfWork, IMapper mapper) : Controller
    {
        [HttpGet("list"), Authorize(Roles = RolesDefinition.ADMIN)]
        public async Task<IActionResult> GetUserRoleConnections([FromQuery] int startIndex = 0, [FromQuery] int endIndex = 100)
        {
            IEnumerable<UserRoleDto>? userRoleConnections = mapper.Map<IEnumerable<UserRoleDto>>(await unitOfWork.UserRole.GetAllItem(new Range(startIndex, endIndex)));

            if (!userRoleConnections.Any())
                return NotFound("user-role connections not found.");

            return Ok(userRoleConnections);
        }

        [HttpGet, Authorize(Roles = RolesDefinition.ADMIN)]
        public async Task<IActionResult> GetUserRoleConnection([FromQuery] Guid id)
        {
            UserRoleDto? userRoleConnection = mapper.Map<UserRoleDto>(await unitOfWork.UserRole.GetItem(new UserRole() { Id = id }));

            if (userRoleConnection == null)
                return NotFound($"User-role connection {id} not found.");

            return Ok(userRoleConnection);
        }

        [HttpPost, Authorize(Roles = RolesDefinition.ADMIN)]
        public async Task<IActionResult> CreateUserRoleConnection([FromQuery] Guid userId, [FromQuery] Guid roleId)
        {
            UserRole userRole = new () {Id = Guid.NewGuid(), UserId = userId, RoleId = roleId };

            UserRoleDto? connect = mapper.Map<UserRoleDto>(await unitOfWork.UserRole.GetConnectionByUserAndRoleId(userRole));

            if (connect != null)
                return BadRequest($"User-role connection already exists.");

            unitOfWork.UserRole.Create(userRole);
            await unitOfWork.SaveAsync();

            return NoContent();
        }

        [HttpDelete, Authorize(Roles = RolesDefinition.ADMIN)]
        public async Task<IActionResult> DeleteUserRoleConnection([FromQuery] Guid id)
        {
            // Поиск сессии по id
            UserRole? connect = await unitOfWork.UserRole.GetItem(new UserRole() { Id = id});

            if (connect == null)
                return BadRequest($"User-role connection {id} not found.");

            // Удаление сессии
            unitOfWork.UserRole.Delete(connect);
            await unitOfWork.SaveAsync();

            return NoContent();
        }        
    }
}