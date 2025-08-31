using AutoMapper;
using CRMService.Interfaces.Repository;
using CRMService.Models.ConfigClass;
using CRMService.Models.Dto.Entity;
using CRMService.Models.Entity;
using CRMService.Models.Enum;
using CRMService.Service.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRMService.Controllers.Entity
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ManufacturerController(IMapper mapper, IUnitOfWork unitOfWork, ManufacturerService service) : Controller
    {
        [HttpGet("list")]
        public async Task<IActionResult> GetManufacturers([FromQuery] long startIndex = 0, CancellationToken ct = default)
        {
            List<Manufacturer> manufacturers = await unitOfWork.Manufacturer.GetItemsByPredicateAndSortById(predicate: m => m.Id >= startIndex, asNoTracking: true, ct: ct);

            return Ok(mapper.Map<List<ManufacturerDto>>(manufacturers));
        }
                
        [HttpPut("update_from_cloud_api"), Authorize(Roles = nameof(UserRole.Admin))]
        public async Task<IActionResult> UpdateManufacturersFromCloudApi([FromQuery] long startIndex = 0, CancellationToken ct = default)
        {
            await service.UpdateManufacturersFromCloudApi(startIndex, LimitConstants.LIMIT_FOR_RETRIEVING_ENTITIES_FROM_API, ct);

            return NoContent();
        }

        [HttpPut("update_from_cloud_db"), Authorize(Roles = nameof(UserRole.Admin))]
        public async Task<IActionResult> UpdateManufacturersFromCloudDb(CancellationToken ct)
        {
            await service.UpdateManufacturersFromCloudDb(ct);

            return NoContent();
        }
    }
}
