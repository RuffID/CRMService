using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CRMService.Application.Service.OkdeskEntity;
using CRMService.Domain.Models.OkdeskEntity;
using CRMService.Domain.Models.Constants;
using CRMService.Application.Abstractions.Database.Repository;
using CRMService.Application.Common.Mapping.OkdeskEntity;
using CRMService.Web.Service.BackgroundServices;

namespace CRMService.Web.Controllers.OkdeskEntity
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class KindController(IUnitOfWork unitOfWork, BackgroundUpdateService backgroundUpdateService) : Controller
    {
        [HttpGet("list")]
        public async Task<IActionResult> GetKinds(CancellationToken ct = default)
        {
            List<Kind> kinds = await unitOfWork.Kind.GetItemsByPredicateAsync(asNoTracking: true, ct: ct);

            return Ok(kinds.ToDto());
        }

        [HttpPut("update_from_cloud_api"), Authorize(Roles = RolesConstants.ADMIN)]
        public IActionResult UpdateKindsFromCloudApi(CancellationToken ct = default)
        {
            bool started = backgroundUpdateService.TryStart(
                nameof(UpdateKindsFromCloudApi),
                (provider, token) => provider.GetRequiredService<KindService>().UpdateKindsFromCloudApi(token));

            return this.ToBackgroundUpdateResponse(started);
        }

        [HttpPut("update_from_cloud_db")]
        public IActionResult UpdateKindsFromCloudDb(CancellationToken ct)
        {
            bool started = backgroundUpdateService.TryStart(
                nameof(UpdateKindsFromCloudDb),
                (provider, token) => provider.GetRequiredService<KindService>().UpdateKindsFromCloudDb(token));

            return this.ToBackgroundUpdateResponse(started);
        }
    }
}
