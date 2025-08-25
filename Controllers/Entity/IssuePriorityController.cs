using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CRMService.Models.ConfigClass;
using Microsoft.Extensions.Options;
using CRMService.Models.Entity;
using CRMService.Service.Entity;
using CRMService.Interfaces.Repository;
using CRMService.Models.Enum;
using CRMService.Models.Dto.Entity;

namespace CRMService.Controllers.Entity
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class IssuePriorityController(IMapper _mapper, IOptions<DatabaseSettings> dbSettings, IUnitOfWork unitOfWork, IssuePriorityService service) : Controller
    {
        [HttpGet("list")]
        public async Task<IActionResult> GetIssuePriorities([FromQuery] int startIndex)
        {
            var priorities = _mapper.Map<IEnumerable<PriorityDto>>(await unitOfWork.IssuePriority.GetItems(startIndex, dbSettings.Value.LimitForRetrievingEntitiesFromDb));

            if (priorities == null || !priorities.Any())
                return NotFound();

            return Ok(priorities);
        }

        [HttpGet]
        public async Task<IActionResult> GetIssuePriority([FromQuery] string code)
        {
            var prioriy = _mapper.Map<PriorityDto>(await unitOfWork.IssuePriority.GetItem( new IssuePriority() { Code = code }, false));

            if (prioriy == null)
                return NotFound();

            return Ok(prioriy);
        }

        [HttpPut("update_from_cloud_api"), Authorize(Roles = nameof(UserRole.Admin))]
        public async Task<IActionResult> UpdateIssuePrioritiesFromCloudApi()
        {
            await service.UpdateIssuePrioritiesFromCloudApi();

            return NoContent();
        }

        [HttpPut("update_from_cloud_db"), Authorize(Roles = nameof(UserRole.Admin))]
        public async Task<IActionResult> UpdateIssuePrioritiesFromCloudDb()
        {
            await service.UpdateIssuePrioritiesFromCloudDb();

            return NoContent();
        }
    }
}