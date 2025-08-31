using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CRMService.Models.ConfigClass;
using CRMService.Service.Entity;
using CRMService.Interfaces.Repository;
using CRMService.Models.Enum;
using CRMService.Models.Dto.Entity;
using CRMService.Models.Entity;

namespace CRMService.Controllers.Entity
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class KindController(IMapper mapper, IUnitOfWork unitOfWork, KindService service) : Controller
    {
        [HttpGet("list")]
        public async Task<IActionResult> GetKinds([FromQuery] int startIndex = 0, CancellationToken ct = default)
        {
            List<Kind> kinds = await unitOfWork.Kind.GetItemsByPredicateAndSortById(predicate: k => k.Id >= startIndex, take: LimitConstants.LIMIT_FOR_RETRIEVING_ENTITIES_FROM_DB, asNoTracking: true, ct: ct);

            return Ok(mapper.Map<List<KindDto>>(kinds));
        }

        [HttpPut("update_from_cloud_api"), Authorize(Roles = nameof(UserRole.Admin))]
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
