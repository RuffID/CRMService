using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CRMService.Models.ConfigClass;
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
    public class IssueStatusController(IMapper mapper, IUnitOfWork unitOfWork, IssueStatusService service) : Controller
    {
        [HttpGet("list")]
        public async Task<IActionResult> GetIssueStatuses([FromQuery] int startIndex, CancellationToken ct)
        {
            List<IssueStatus> statuses = await unitOfWork.IssueStatus.GetItemsByPredicateAndSortById(predicate: s => s.Id >= startIndex, take: LimitConstants.LIMIT_FOR_RETRIEVING_ENTITIES_FROM_DB, asNoTracking: true, ct: ct);

            return Ok(mapper.Map<List<StatusDto>>(statuses));
        }

        [HttpGet]
        public async Task<IActionResult> GetIssueStatus([FromQuery] string code, CancellationToken ct)
        {
            IssueStatus? status = await unitOfWork.IssueStatus.GetItemByPredicate(ip => ip.Code == code, true, ct);

            if (status == null)
                return NotFound();

            return Ok(mapper.Map<StatusDto>(status));
        }

        [HttpPut("update_from_cloud_api"), Authorize(Roles = nameof(UserRole.Admin))]
        public async Task<IActionResult> UpdateIssueStatusesFromCloudApi(CancellationToken ct)
        {
            await service.UpdateIssueStatusesFromCloudApi(ct);

            return NoContent();
        }

        [HttpPut("update_from_cloud_db"), Authorize(Roles = nameof(UserRole.Admin))]
        public async Task<IActionResult> UpdateIssueStatusesFromCloudDb(CancellationToken ct)
        {
            await service.UpdateIssueStatusesFromCloudDb(ct);

            return NoContent();
        }
    }
}