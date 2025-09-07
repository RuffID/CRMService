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
    public class IssueTypeController(IMapper mapper, IUnitOfWork unitOfWork, IssueTypeService service) : Controller
    {
        [HttpGet("list")]
        public async Task<IActionResult> GetIssueTypes([FromQuery] int startIndex, CancellationToken ct)
        {
            List<IssueType> types = await unitOfWork.IssueType.GetItemsByPredicateAndSortById(predicate: t => t.Id >= startIndex, take: LimitConstants.LIMIT_FOR_RETRIEVING_ENTITIES_FROM_DB, asNoTracking: true, ct: ct);

            return Ok(mapper.Map<List<TaskTypeDto>>(types));
        }

        [HttpGet]
        public async Task<IActionResult> GetIssueType([FromQuery] string code, CancellationToken ct)
        {
            IssueType? type = await unitOfWork.IssueType.GetItemByPredicate(it => it.Code == code, true, ct);

            if (type == null)
                return NotFound();

            return Ok(mapper.Map<TaskTypeDto>(type));
        }

        [HttpPut("update_from_cloud_api"), Authorize(Roles = nameof(UserRole.Admin))]
        public async Task<IActionResult> UpdateIssueTypesFromCloudApi(CancellationToken ct)
        {
            await service.UpdateIssueTypesFromCloudApi(ct);

            return NoContent();
        }

        [HttpPut("update_from_cloud_db"), Authorize(Roles = nameof(UserRole.Admin))]
        public async Task<IActionResult> UpdatetIssueTypesFromCloudDb(CancellationToken ct)
        {
            await service.UpdateIssueTypesFromCloudDb(ct);

            return NoContent();
        }
    }
}