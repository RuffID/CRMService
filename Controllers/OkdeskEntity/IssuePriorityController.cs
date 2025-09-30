using CRMService.Interfaces.Repository;
using CRMService.Models.Constants;
using CRMService.Models.Dto.Mappers;
using CRMService.Models.Dto.Mappers.OkdeskEntity;
using CRMService.Models.OkdeskEntity;
using CRMService.Service.OkdeskEntity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRMService.Controllers.OkdeskEntity
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class IssuePriorityController(IUnitOfWork unitOfWork, IssuePriorityService service) : Controller
    {
        [HttpGet("list")]
        public async Task<IActionResult> GetIssuePriorities([FromQuery] int startIndex, CancellationToken ct)
        {
            List<IssuePriority> priorities = await unitOfWork.IssuePriority.GetItemsByPredicateAndSortById(predicate: p => p.Id >= startIndex, take: LimitConstants.LIMIT_FOR_RETRIEVING_ENTITIES_FROM_DB, asNoTracking: true, ct: ct);

            return Ok(priorities.ToDto());
        }

        [HttpGet]
        public async Task<IActionResult> GetIssuePriority([FromQuery] string code, CancellationToken ct)
        {
            IssuePriority? prioriy = await unitOfWork.IssuePriority.GetItemByPredicate(ip => ip.Code == code, true, ct);

            if (prioriy == null)
                return NotFound();

            return Ok(prioriy.ToDto());
        }

        [HttpPut("update_from_cloud_api"), Authorize(Roles = RolesConstants.ADMIN)]
        public async Task<IActionResult> UpdateIssuePrioritiesFromCloudApi(CancellationToken ct)
        {
            await service.UpdateIssuePrioritiesFromCloudApi(ct);

            return NoContent();
        }

        [HttpPut("update_from_cloud_db"), Authorize(Roles = RolesConstants.ADMIN)]
        public async Task<IActionResult> UpdateIssuePrioritiesFromCloudDb(CancellationToken ct)
        {
            await service.UpdateIssuePrioritiesFromCloudDb(ct);

            return NoContent();
        }
    }
}