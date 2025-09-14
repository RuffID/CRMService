using CRMService.DataBase;
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
    public class IssueStatusController(IUnitOfWork unitOfWork, IssueStatusService service) : Controller
    {
        [HttpGet("list")]
        public async Task<IActionResult> GetIssueStatuses([FromQuery] int startIndex, CancellationToken ct)
        {
            List<IssueStatus> statuses = await unitOfWork.IssueStatus.GetItemsByPredicateAndSortById(predicate: s => s.Id >= startIndex, take: LimitConstants.LIMIT_FOR_RETRIEVING_ENTITIES_FROM_DB, asNoTracking: true, ct: ct);

            List<StatusDto> dtos = statuses.Select(p => new StatusDto()
            {
                Id = p.Id,
                Name = p.Name,
                Code = p.Code
            }).ToList();

            return Ok(dtos);
        }

        [HttpGet]
        public async Task<IActionResult> GetIssueStatus([FromQuery] string code, CancellationToken ct)
        {
            IssueStatus? status = await unitOfWork.IssueStatus.GetItemByPredicate(ip => ip.Code == code, true, ct);

            if (status == null)
                return NotFound();

            StatusDto dto = new()
            {
                Id = status.Id,
                Name = status.Name,
                Code = status.Code,
            };

            return Ok(dto);
        }

        [HttpPut("update_from_cloud_api"), Authorize(Roles = RolesDefinitionConstants.ADMIN)]
        public async Task<IActionResult> UpdateIssueStatusesFromCloudApi(CancellationToken ct)
        {
            await service.UpdateIssueStatusesFromCloudApi(ct);

            return NoContent();
        }

        [HttpPut("update_from_cloud_db"), Authorize(Roles = RolesDefinitionConstants.ADMIN)]
        public async Task<IActionResult> UpdateIssueStatusesFromCloudDb(CancellationToken ct)
        {
            await service.UpdateIssueStatusesFromCloudDb(ct);

            return NoContent();
        }
    }
}