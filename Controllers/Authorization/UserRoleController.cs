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
    public class UserRoleController(IUnitOfWork unitOfWork, IMapper mapper) : Controller
    {
        [HttpGet("list"), Authorize(Roles = RolesDefinition.ADMIN)]
        public async Task<IActionResult> GetUserRoleConnections([FromQuery] int startIndex = 0, [FromQuery] int limit = LimitConstants.LIMIT_FOR_RETRIEVING_ENTITIES_FROM_DB, CancellationToken ct = default)
        {
            List<UserRole> userRoleConnections = await unitOfWork.UserRole.GetItemsByPredicate(skip: startIndex, take: limit, asNoTracking: true, ct: ct);

            return Ok(mapper.Map<IEnumerable<UserRoleDto>>(userRoleConnections));
        }

        [HttpGet, Authorize(Roles = RolesDefinition.ADMIN)]
        public async Task<IActionResult> GetUserRoleConnection([FromQuery] Guid userId, [FromQuery] Guid roleId, CancellationToken ct)
        {
            UserRole? userRoleConnection = await unitOfWork.UserRole.GetItemByPredicate(predicate: ur => ur.UserId == userId && ur.RoleId == roleId, asNoTracking: true, ct: ct);

            if (userRoleConnection == null)
                return NotFound($"User-role connection by userId: {userId} and roleId: {roleId} - not found.");

            return Ok(mapper.Map<UserRoleDto>(userRoleConnection));
        }

        [HttpPost, Authorize(Roles = RolesDefinition.ADMIN)]
        public async Task<IActionResult> CreateUserRoleConnection([FromQuery] Guid userId, [FromQuery] Guid roleId, CancellationToken ct)
        {
            UserRole? connection = await unitOfWork.UserRole.GetItemByPredicate(predicate: ur => ur.UserId == userId && ur.RoleId == roleId, asNoTracking: true, ct: ct);

            if (connection != null)
                return BadRequest($"User-role connection by userId: {userId} and roleId: {roleId} - already exists.");

            connection = new()
            {
                UserId = userId,
                RoleId = roleId
            };

            unitOfWork.UserRole.Create(connection);
            await unitOfWork.SaveAsync();

            return NoContent();
        }

        [HttpDelete, Authorize(Roles = RolesDefinition.ADMIN)]
        public async Task<IActionResult> DeleteUserRoleConnection([FromQuery] Guid userId, [FromQuery] Guid roleId, CancellationToken ct)
        {
            UserRole? connection = await unitOfWork.UserRole.GetItemByPredicate(predicate: ur => ur.UserId == userId && ur.RoleId == roleId, asNoTracking: true, ct: ct);

            if (connection == null)
                return BadRequest($"User-role connection by userId: {userId} and roleId: {roleId} - not found.");

            unitOfWork.UserRole.Delete(connection);
            await unitOfWork.SaveAsync();

            return NoContent();
        }        
    }
}