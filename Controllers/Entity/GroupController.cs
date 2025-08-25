using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using CRMService.Models.ConfigClass;
using AutoMapper;
using CRMService.Service.Entity;
using CRMService.Interfaces.Repository;
using CRMService.Models.Enum;
using CRMService.Models.Dto.Entity;

namespace CRMService.Controllers.Entity
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class GroupController(IOptions<OkdeskSettings> okdSettings, IUnitOfWork unitOfWork, IMapper mapper, GroupService service) : Controller
    {

        [HttpGet("list")]
        public async Task<IActionResult> GetGroups([FromQuery] int startIndex = 0)
        {
            var groups = mapper.Map<IEnumerable<GroupDto>>(await unitOfWork.Group.GetItems(startIndex, okdSettings.Value.LimitForRetrievingEntitiesFromApi));

            if (groups == null || !groups.Any())
                return NotFound();

            return Ok(groups);
        }

        [HttpPut("update_from_cloud_api"), Authorize(Roles = nameof(UserRole.Admin))]
        public async Task<IActionResult> UpdateGroupsFromCloudApi()
        {
            await service.UpdateGroupsFromCloudApi();

            return NoContent();
        }

        [HttpPut("update_from_cloud_db"), Authorize(Roles = nameof(UserRole.Admin))]
        public async Task<IActionResult> UpdateGroupsFromCloudDb()
        {
            await service.UpdateGroupsFromCloudDb();

            return NoContent();
        }
    }
}