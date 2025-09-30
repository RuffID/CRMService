using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CRMService.Models.OkdeskEntity;
using CRMService.Service.OkdeskEntity;
using CRMService.Interfaces.Repository;
using CRMService.Models.Constants;
using CRMService.Models.Dto.Mappers;
using CRMService.Models.Dto.Mappers.OkdeskEntity;

namespace CRMService.Controllers.OkdeskEntity
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

            return Ok(types.ToDto());
        }

        [HttpGet]
        public async Task<IActionResult> GetIssueType([FromQuery] string code, CancellationToken ct)
        {
            IssueType? type = await unitOfWork.IssueType.GetItemByPredicate(it => it.Code == code, true, ct);

            if (type == null)
                return NotFound();

            return Ok(type.ToDto());
        }

        [HttpPut("update_from_cloud_api"), Authorize(Roles = RolesConstants.ADMIN)]
        public async Task<IActionResult> UpdateIssueTypesFromCloudApi(CancellationToken ct)
        {
            await service.UpdateIssueTypesFromCloudApi(ct);

            return NoContent();
        }

        [HttpPut("update_from_cloud_db"), Authorize(Roles = RolesConstants.ADMIN)]
        public async Task<IActionResult> UpdatetIssueTypesFromCloudDb(CancellationToken ct)
        {
            await service.UpdateIssueTypesFromCloudDb(ct);

            return NoContent();
        }
    }
}