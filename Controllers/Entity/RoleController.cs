using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CRMService.Service.Entity;
using CRMService.Interfaces.Repository;
using CRMService.Models.ConfigClass;
using CRMService.Models.Entity;
using CRMService.Models.Dto.Entity;

namespace CRMService.Controllers.Entity
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class RoleController(IUnitOfWork unitOfWork, RoleService service) : Controller
    {
        [HttpGet("list")]
        public async Task<IActionResult> GetRoles([FromQuery] int startIndex = 0, CancellationToken ct = default)
        {
            List<OkdeskRole> roles = await unitOfWork.OkdeskRole.GetItemsByPredicateAndSortById(predicate: r => r.Id >= startIndex, take: LimitConstants.LIMIT_FOR_RETRIEVING_ENTITIES_FROM_DB, asNoTracking: true, ct: ct);

            List<OkdeskRoleDto> dtos = roles.Select(r => new OkdeskRoleDto()
            {
                Id = r.Id,
                Name = r.Name
            }).ToList();

            return Ok(dtos);
        }

        [HttpPut("update_from_cloud_api"), Authorize(Roles = RolesDefinitionConstants.ADMIN)]
        public async Task<IActionResult> UpdateRolesFromCloudApi(CancellationToken ct)
        {
            await service.UpdateRolesFromCloudApi(ct);

            return NoContent();
        }
    }
}
