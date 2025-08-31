using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CRMService.Models.Entity;
using CRMService.Service.Entity;
using CRMService.Interfaces.Repository;
using CRMService.Models.Enum;
using CRMService.Models.Dto.Entity;
using CRMService.Models.ConfigClass;

namespace CRMService.Controllers.Entity
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class IssuePriorityController(IMapper _mapper, IUnitOfWork unitOfWork, IssuePriorityService service) : Controller
    {
        [HttpGet("list")]
        public async Task<IActionResult> GetIssuePriorities([FromQuery] int startIndex, CancellationToken ct)
        {
            List<IssuePriority> priorities = await unitOfWork.IssuePriority.GetItemsByPredicateAndSortById(predicate: p => p.Id >= startIndex, take: LimitConstants.LIMIT_FOR_RETRIEVING_ENTITIES_FROM_DB, asNoTracking: true, ct: ct);

            return Ok(_mapper.Map<List<PriorityDto>>(priorities));
        }

        [HttpGet]
        public async Task<IActionResult> GetIssuePriority([FromQuery] string code, CancellationToken ct)
        {
            IssuePriority? prioriy = await unitOfWork.IssuePriority.GetItemByCode(code, false, ct);

            if (prioriy == null)
                return NotFound();

            return Ok(_mapper.Map<PriorityDto>(prioriy));
        }

        [HttpPut("update_from_cloud_api"), Authorize(Roles = nameof(UserRole.Admin))]
        public async Task<IActionResult> UpdateIssuePrioritiesFromCloudApi(CancellationToken ct)
        {
            await service.UpdateIssuePrioritiesFromCloudApi(ct);

            return NoContent();
        }

        [HttpPut("update_from_cloud_db"), Authorize(Roles = nameof(UserRole.Admin))]
        public async Task<IActionResult> UpdateIssuePrioritiesFromCloudDb(CancellationToken ct)
        {
            await service.UpdateIssuePrioritiesFromCloudDb(ct);

            return NoContent();
        }
    }
}