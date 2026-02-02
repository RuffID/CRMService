using CRMService.Interfaces.Repository;
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
    public class ManufacturerController(IUnitOfWork unitOfWork, ManufacturerService service) : Controller
    {
        [HttpGet("list")]
        public async Task<IActionResult> GetManufacturers([FromQuery] long startIndex = 0, CancellationToken ct = default)
        {
            List<Manufacturer> manufacturers = await unitOfWork.Manufacturer.GetItemsByPredicateAsync(predicate: m => m.Id >= startIndex, asNoTracking: true, ct: ct);

            return Ok(manufacturers.ToDto());
        }

        [HttpPut("update_from_cloud_api"), Authorize(Roles = RolesConstants.ADMIN)]
        public async Task<IActionResult> UpdateManufacturersFromCloudApi([FromQuery] long startIndex = 0, CancellationToken ct = default)
        {
            await service.UpdateManufacturersFromCloudApi(startIndex, LimitConstants.LIMIT_FOR_RETRIEVING_ENTITIES_FROM_API, ct);

            return NoContent();
        }

        [HttpPut("update_from_cloud_db"), Authorize(Roles = RolesConstants.ADMIN)]
        public async Task<IActionResult> UpdateManufacturersFromCloudDb(CancellationToken ct)
        {
            await service.UpdateManufacturersFromCloudDb(ct);

            return NoContent();
        }
    }
}
