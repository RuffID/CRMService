using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CRMService.Application.Service.OkdeskEntity;
using CRMService.Domain.Models.OkdeskEntity;
using CRMService.Domain.Models.Constants;
using CRMService.Application.Abstractions.Database.Repository;
using CRMService.Application.Common.Mapping.OkdeskEntity;

namespace CRMService.Web.Controllers.OkdeskEntity
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class KindController(IUnitOfWork unitOfWork, KindService service) : Controller
    {
        [HttpGet("list")]
        public async Task<IActionResult> GetKinds(CancellationToken ct = default)
        {
            List<Kind> kinds = await unitOfWork.Kind.GetItemsByPredicateAsync(asNoTracking: true, ct: ct);

            return Ok(kinds.ToDto());
        }

        [HttpPut("update_from_cloud_api"), Authorize(Roles = RolesConstants.ADMIN)]
        public async Task<IActionResult> UpdateKindsFromCloudApi(CancellationToken ct = default)
        {
            await service.UpdateKindsFromCloudApi(ct);

            return NoContent();
        }

        [HttpPut("update_from_cloud_db")]
        public async Task<IActionResult> UpdateKindsFromCloudDb(CancellationToken ct)
        {
            await service.UpdateKindsFromCloudDb(ct);

            return NoContent();
        }
    }
}