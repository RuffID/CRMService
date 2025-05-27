using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using CRMService.Models.ConfigClass;
using AutoMapper;
using CRMService.Dto;
using CRMService.Core;
using CRMService.Service.Entity;
using CRMService.Interfaces.Repository;

namespace CRMService.Controllers.Entity
{
    [Authorize]
    [Route("api/crm/[controller]")]
    [ApiController]
    public class GroupController(IOptions<OkdeskSettings> okdSettings, IUnitOfWorkEntities unitOfWork, IMapper mapper, GroupService service) : Controller
    {

        [HttpGet("list")]
        public async Task<IActionResult> GetGroups([FromQuery] int startIndex = 0)
        {
            var groups = mapper.Map<IEnumerable<GroupDto>>(await unitOfWork.Group.GetItems(startIndex, okdSettings.Value.LimitForRetrievingEntitiesFromApi));

            if (groups == null || !groups.Any())
                return NotFound();

            return Ok(groups);
        }

        [HttpPut("update_from_cloud_api"), Authorize(Roles = UserRole.ADMIN)]
        public async Task<IActionResult> UpdateGroupsFromCloudApi()
        {
            await service.UpdateGroupsFromCloudApi();

            return NoContent();
        }

        [HttpPut("update_from_cloud_db"), Authorize(Roles = UserRole.ADMIN)]
        public async Task<IActionResult> UpdateGroupsFromCloudDb()
        {
            await service.UpdateGroupsFromCloudDb();

            return NoContent();
        }
    }
}