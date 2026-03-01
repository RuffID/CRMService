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
    public class ManufacturerController(IUnitOfWork unitOfWork, ManufacturerService service) : Controller
    {
        [HttpGet("list")]
        public async Task<IActionResult> GetManufacturers(CancellationToken ct = default)
        {
            List<Manufacturer> manufacturers = await unitOfWork.Manufacturer.GetItemsByPredicateAsync(asNoTracking: true, ct: ct);

            return Ok(manufacturers.ToDto());
        }

        [HttpPut("update_from_cloud_api"), Authorize(Roles = RolesConstants.ADMIN)]
        public async Task<IActionResult> UpdateManufacturersFromCloudApi(CancellationToken ct = default)
        {
            await service.UpdateManufacturersFromCloudApi(ct);

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