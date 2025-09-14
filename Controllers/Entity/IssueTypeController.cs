using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CRMService.Models.Entity;
using CRMService.Service.Entity;
using CRMService.Interfaces.Repository;
using CRMService.Models.Dto.Entity;
using CRMService.Models.ConfigClass;

namespace CRMService.Controllers.Entity
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class IssueTypeController(IUnitOfWork unitOfWork, IssueTypeService service) : Controller
    {
        [HttpGet("list")]
        public async Task<IActionResult> GetIssueTypes([FromQuery] int startIndex, CancellationToken ct)
        {
            List<IssueType> types = await unitOfWork.IssueType.GetItemsByPredicateAndSortById(predicate: t => t.Id >= startIndex, take: LimitConstants.LIMIT_FOR_RETRIEVING_ENTITIES_FROM_DB, asNoTracking: true, ct: ct);

            List<TaskTypeDto> dtos = types.Select(p => new TaskTypeDto()
            {
                Id = p.Id,
                Name = p.Name,
                Code = p.Code
            }).ToList();

            return Ok(dtos);
        }

        [HttpGet]
        public async Task<IActionResult> GetIssueType([FromQuery] string code, CancellationToken ct)
        {
            IssueType? type = await unitOfWork.IssueType.GetItemByPredicate(it => it.Code == code, true, ct);

            if (type == null)
                return NotFound();

            TaskTypeDto dto = new()
            {
                Id = type.Id,
                Name = type.Name,
                Code = type.Code
            };

            return Ok(dto);
        }

        [HttpPut("update_from_cloud_api"), Authorize(Roles = RolesDefinitionConstants.ADMIN)]
        public async Task<IActionResult> UpdateIssueTypesFromCloudApi(CancellationToken ct)
        {
            await service.UpdateIssueTypesFromCloudApi(ct);

            return NoContent();
        }

        [HttpPut("update_from_cloud_db"), Authorize(Roles = RolesDefinitionConstants.ADMIN)]
        public async Task<IActionResult> UpdatetIssueTypesFromCloudDb(CancellationToken ct)
        {
            await service.UpdateIssueTypesFromCloudDb(ct);

            return NoContent();
        }
    }
}