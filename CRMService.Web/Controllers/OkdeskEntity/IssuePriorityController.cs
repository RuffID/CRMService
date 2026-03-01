using CRMService.Application.Abstractions.Database.Repository;
using CRMService.Domain.Models.Constants;
using CRMService.Domain.Models.OkdeskEntity;
using CRMService.Application.Service.OkdeskEntity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CRMService.Application.Common.Mapping.OkdeskEntity;

namespace CRMService.Web.Controllers.OkdeskEntity
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class IssuePriorityController(IUnitOfWork unitOfWork, IssuePriorityService service) : Controller
    {
        [HttpGet("list")]
        public async Task<IActionResult> GetIssuePriorities(CancellationToken ct)
        {
            return JsonResultMapper.ToJsonResult(await service.GetIssuePrioritiesAsync(ct));
        }

        [HttpGet]
        public async Task<IActionResult> GetIssuePriority([FromQuery] string code, CancellationToken ct)
        {
            IssuePriority? prioriy = await unitOfWork.IssuePriority.GetItemByPredicateAsync(ip => ip.Code == code, asNoTracking: true, ct: ct);

            if (prioriy == null)
                return NotFound();

            return Ok(prioriy.ToDto());
        }

        [HttpPut("update_from_cloud_api"), Authorize(Roles = RolesConstants.ADMIN)]
        public async Task<IActionResult> UpdateIssuePrioritiesFromCloudApi(CancellationToken ct)
        {
            await service.UpdateIssuePrioritiesFromCloudApi(ct);

            return NoContent();
        }

        [HttpPut("update_from_cloud_db"), Authorize(Roles = RolesConstants.ADMIN)]
        public async Task<IActionResult> UpdateIssuePrioritiesFromCloudDb(CancellationToken ct)
        {
            await service.UpdateIssuePrioritiesFromCloudDb(ct);

            return NoContent();
        }
    }
}