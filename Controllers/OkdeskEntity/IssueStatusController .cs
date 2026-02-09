using CRMService.Abstractions.Database.Repository;
using CRMService.Models.Constants;
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
    public class IssueStatusController(IUnitOfWork unitOfWork, IssueStatusService service) : Controller
    {
        [HttpGet("list")]
        public async Task<IActionResult> GetIssueStatuses(CancellationToken ct)
        {
            return Ok((await service.GetIssueStatusesAsync(ct)).Data);
        }

        [HttpGet]
        public async Task<IActionResult> GetIssueStatus([FromQuery] string code, CancellationToken ct)
        {
            IssueStatus? status = await unitOfWork.IssueStatus.GetItemByPredicateAsync(ip => ip.Code == code, asNoTracking: true, ct: ct);

            if (status == null)
                return NotFound();

            return Ok(status.ToDto());
        }

        [HttpPut("update_from_cloud_api"), Authorize(Roles = RolesConstants.ADMIN)]
        public async Task<IActionResult> UpdateIssueStatusesFromCloudApi(CancellationToken ct)
        {
            await service.UpdateIssueStatusesFromCloudApi(ct);

            return NoContent();
        }

        [HttpPut("update_from_cloud_db"), Authorize(Roles = RolesConstants.ADMIN)]
        public async Task<IActionResult> UpdateIssueStatusesFromCloudDb(CancellationToken ct)
        {
            await service.UpdateIssueStatusesFromCloudDb(ct);

            return NoContent();
        }
    }
}