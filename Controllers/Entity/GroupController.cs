using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CRMService.Models.ConfigClass;
using AutoMapper;
using CRMService.Service.Entity;
using CRMService.Interfaces.Repository;
using CRMService.Models.Enum;
using CRMService.Models.Dto.Entity;
using CRMService.Models.Entity;

namespace CRMService.Controllers.Entity
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class GroupController(IUnitOfWork unitOfWork, IMapper mapper, GroupService service) : Controller
    {

        [HttpGet("list")]
        public async Task<IActionResult> GetGroups([FromQuery] int startIndex = 0, CancellationToken ct = default)
        {
            List<Group> groups = await unitOfWork.Group.GetItemsByPredicateAndSortById(predicate: g => g.Id >= startIndex, take: LimitConstants.LIMIT_FOR_RETRIEVING_ENTITIES_FROM_DB, asNoTracking: true, ct: ct);

            return Ok(mapper.Map<List<GroupDto>>(groups));
        }

        [HttpPut("update_from_cloud_api"), Authorize(Roles = nameof(UserRole.Admin))]
        public async Task<IActionResult> UpdateGroupsFromCloudApi(CancellationToken ct)
        {
            await service.UpdateGroupsFromCloudApi(ct);

            return NoContent();
        }

        [HttpPut("update_from_cloud_db"), Authorize(Roles = nameof(UserRole.Admin))]
        public async Task<IActionResult> UpdateGroupsFromCloudDb(CancellationToken ct)
        {
            await service.UpdateGroupsFromCloudDb(ct);

            return NoContent();
        }
    }
}