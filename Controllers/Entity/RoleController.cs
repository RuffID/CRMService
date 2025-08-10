using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using CRMService.Models.ConfigClass;
using CRMService.Core;
using CRMService.Service.Entity;
using CRMService.Interfaces.Repository;

namespace CRMService.Controllers.Entity
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class RoleController(IOptions<DatabaseSettings> _dbSettings, IUnitOfWorkEntities unitOfWork, RoleService service) : Controller
    {

        [HttpGet("list")]
        public async Task<IActionResult> GetRoles([FromQuery] int startIndex = 0)
        {
            var roles = await unitOfWork.Role.GetItems(startIndex, _dbSettings.Value.LimitForRetrievingEntitiesFromDb);

            if (roles == null || !roles.Any())
                return NotFound("Roles not found.");

            return Ok(roles);
        }

        [HttpPut("update_from_cloud_api"), Authorize(Roles = UserRole.ADMIN)]
        public async Task<IActionResult> UpdateRolesFromCloudApi()
        {
            await service.UpdateRolesFromCloudApi();

            return NoContent();
        }
    }
}
