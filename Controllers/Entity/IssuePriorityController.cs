using CRMService.Interfaces.Repository;
using CRMService.Models.ConfigClass;
using CRMService.Models.Dto.Entity;
using CRMService.Models.Entity;
using CRMService.Service.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRMService.Controllers.Entity
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

            List<PriorityDto> dtos = priorities.Select(p => new PriorityDto()
            {
                Id = p.Id,
                Name = p.Name,
                Code = p.Code
            }).ToList();

            return Ok(dtos);
        }

        [HttpGet]
        public async Task<IActionResult> GetIssuePriority([FromQuery] string code, CancellationToken ct)
        {
            IssuePriority? prioriy = await unitOfWork.IssuePriority.GetItemByPredicate(ip => ip.Code == code, true, ct);

            if (prioriy == null)
                return NotFound();

            PriorityDto dto = new ()
            {
                Id = prioriy.Id,
                Name = prioriy.Name,
                Code = prioriy.Code
            };

            return Ok(dto);
        }

        [HttpPut("update_from_cloud_api"), Authorize(Roles = RolesDefinitionConstants.ADMIN)]
        public async Task<IActionResult> UpdateIssuePrioritiesFromCloudApi(CancellationToken ct)
        {
            await service.UpdateIssuePrioritiesFromCloudApi(ct);

            return NoContent();
        }

        [HttpPut("update_from_cloud_db"), Authorize(Roles = RolesDefinitionConstants.ADMIN)]
        public async Task<IActionResult> UpdateIssuePrioritiesFromCloudDb(CancellationToken ct)
        {
            await service.UpdateIssuePrioritiesFromCloudDb(ct);

            return NoContent();
        }
    }
}