using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CRMService.Service.OkdeskEntity;
using CRMService.Interfaces.Repository;
using CRMService.Models.OkdeskEntity;
using CRMService.Models.Constants;
using CRMService.Models.Dto.Mappers.OkdeskEntity;
using CRMService.Models.Dto.Mappers.Authorize;

namespace CRMService.Controllers.OkdeskEntity
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class GroupController(IUnitOfWork unitOfWork, GroupService service) : Controller
    {
        [HttpGet("list")]
        public async Task<IActionResult> GetGroups([FromQuery] int startIndex = 0, CancellationToken ct = default)
        {
            List<Group> groups = await unitOfWork.Group.GetItemsByPredicateAndSortById(predicate: g => g.Id >= startIndex, take: LimitConstants.LIMIT_FOR_RETRIEVING_ENTITIES_FROM_DB, asNoTracking: true, ct: ct);

            return Ok(groups.ToDto());
        }

        [HttpPut("update_from_cloud_api"), Authorize(Roles = RolesConstants.ADMIN)]
        public async Task<IActionResult> UpdateGroupsFromCloudApi(CancellationToken ct)
        {
            await service.UpdateGroupsFromCloudApi(ct);

            return NoContent();
        }

        [HttpPut("update_connections_with_employees_from_cloud_api"), Authorize(Roles = RolesConstants.ADMIN)]
        public async Task<IActionResult> UpdateGroupConnectionsWithEmployeeFromCloudApi(CancellationToken ct)
        {
            await service.UpsertEmployeeGroupConnectionsFromApi(ct);

            return NoContent();
        }
    }
}