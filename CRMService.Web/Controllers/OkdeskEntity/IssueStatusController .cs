using CRMService.Application.Abstractions.Database.Repository;
using CRMService.Domain.Models.Constants;
using CRMService.Domain.Models.OkdeskEntity;
using CRMService.Application.Service.OkdeskEntity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CRMService.Application.Common.Mapping.OkdeskEntity;
using CRMService.Web.Service.BackgroundServices;
using CRMService.Web.Core.Mappers;

namespace CRMService.Web.Controllers.OkdeskEntity
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class IssueStatusController(IssueStatusService service, BackgroundUpdateService backgroundUpdateService) : Controller
    {
        [HttpGet("list")]
        public async Task<IActionResult> GetIssueStatuses(CancellationToken ct)
        {
            return JsonResultMapper.ToJsonResult(await service.GetIssueStatusesAsync(ct));
        }

        [HttpGet]
        public async Task<IActionResult> GetIssueStatus([FromQuery] string code, CancellationToken ct)
        {
            IssueStatus? status = await service.GetIssueStatusAsync(code, ct);

            if (status == null)
                return NotFound();

            return Ok(status.ToDto());
        }

        [HttpPut("update_from_cloud_api"), Authorize(Roles = RolesConstants.ADMIN)]
        public IActionResult UpdateIssueStatusesFromCloudApi(CancellationToken ct)
        {
            bool started = backgroundUpdateService.TryStart(
                nameof(UpdateIssueStatusesFromCloudApi),
                (provider, token) => provider.GetRequiredService<IssueStatusService>().UpdateIssueStatusesFromCloudApi(token));

            return this.ToBackgroundUpdateResponse(started);
        }

        [HttpPut("update_from_cloud_db"), Authorize(Roles = RolesConstants.ADMIN)]
        public IActionResult UpdateIssueStatusesFromCloudDb(CancellationToken ct)
        {
            bool started = backgroundUpdateService.TryStart(
                nameof(UpdateIssueStatusesFromCloudDb),
                (provider, token) => provider.GetRequiredService<IssueStatusService>().UpdateIssueStatusesFromCloudDb(token));

            return this.ToBackgroundUpdateResponse(started);
        }
    }
}
