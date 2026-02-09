using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CRMService.Service.OkdeskEntity;
using CRMService.Models.OkdeskEntity;
using CRMService.Models.Constants;
using CRMService.Models.Dto.Mappers.OkdeskEntity;
using CRMService.Abstractions.Database.Repository;

namespace CRMService.Controllers.OkdeskEntity
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
