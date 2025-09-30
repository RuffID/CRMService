using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CRMService.Service.OkdeskEntity;
using CRMService.Interfaces.Repository;
using CRMService.Models.OkdeskEntity;
using CRMService.Models.Constants;
using CRMService.Models.Dto.Mappers;
using CRMService.Models.Dto.Mappers.OkdeskEntity;

namespace CRMService.Controllers.OkdeskEntity
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class KindController(IUnitOfWork unitOfWork, KindService service) : Controller
    {
        [HttpGet("list")]
        public async Task<IActionResult> GetKinds([FromQuery] int startIndex = 0, CancellationToken ct = default)
        {
            List<Kind> kinds = await unitOfWork.Kind.GetItemsByPredicateAndSortById(predicate: k => k.Id >= startIndex, take: LimitConstants.LIMIT_FOR_RETRIEVING_ENTITIES_FROM_DB, asNoTracking: true, ct: ct);

            return Ok(kinds.ToDto());
        }

        [HttpPut("update_from_cloud_api"), Authorize(Roles = RolesConstants.ADMIN)]
        public async Task<IActionResult> UpdateKindsFromCloudApi([FromQuery] int startIndex = 0, CancellationToken ct = default)
        {
            await service.UpdateKindsFromCloudApi(startIndex, LimitConstants.LIMIT_FOR_RETRIEVING_ENTITIES_FROM_API, ct);

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
