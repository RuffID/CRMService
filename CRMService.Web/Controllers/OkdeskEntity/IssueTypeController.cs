using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CRMService.Domain.Models.OkdeskEntity;
using CRMService.Application.Service.OkdeskEntity;
using CRMService.Domain.Models.Constants;
using CRMService.Application.Abstractions.Database.Repository;
using CRMService.Application.Common.Mapping.OkdeskEntity;
using CRMService.Web.Service.BackgroundServices;
using CRMService.Web.Core.Mappers;

namespace CRMService.Web.Controllers.OkdeskEntity
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class IssueTypeController(IssueTypeService service, BackgroundUpdateService backgroundUpdateService) : Controller
    {
        [HttpGet("list")]
        public async Task<IActionResult> GetIssueTypes(CancellationToken ct)
        {
            return JsonResultMapper.ToJsonResult(await service.GetTypes(ct));
        }

        [HttpGet]
        public async Task<IActionResult> GetIssueType([FromQuery] string code, CancellationToken ct)
        {
            IssueType? type = await service.GetIssueTypeAsync(code, ct);

            if (type == null)
                return NotFound();

            return Ok(type.ToDto());
        }

        [HttpPut("update_from_cloud_api"), Authorize(Roles = RolesConstants.ADMIN)]
        public IActionResult UpdateIssueTypesFromCloudApi(CancellationToken ct)
        {
            bool started = backgroundUpdateService.TryStart(
                nameof(UpdateIssueTypesFromCloudApi),
                (provider, token) => provider.GetRequiredService<IssueTypeService>().UpdateIssueTypesFromCloudApi(token));

            return this.ToBackgroundUpdateResponse(started);
        }

        [HttpPut("update_from_cloud_db"), Authorize(Roles = RolesConstants.ADMIN)]
        public IActionResult UpdatetIssueTypesFromCloudDb(CancellationToken ct)
        {
            bool started = backgroundUpdateService.TryStart(
                nameof(UpdatetIssueTypesFromCloudDb),
                (provider, token) => provider.GetRequiredService<IssueTypeService>().UpdateIssueTypesFromCloudDb(token));

            return this.ToBackgroundUpdateResponse(started);
        }
    }
}
