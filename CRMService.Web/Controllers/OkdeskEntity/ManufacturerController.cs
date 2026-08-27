using CRMService.Application.Abstractions.Database.Repository;
using CRMService.Domain.Models.Constants;
using CRMService.Domain.Models.OkdeskEntity;
using CRMService.Application.Service.OkdeskEntity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CRMService.Application.Common.Mapping.OkdeskEntity;
using CRMService.Web.Service.BackgroundServices;

namespace CRMService.Web.Controllers.OkdeskEntity
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ManufacturerController(ManufacturerService service, BackgroundUpdateService backgroundUpdateService) : Controller
    {
        [HttpGet("list")]
        public async Task<IActionResult> GetManufacturers(CancellationToken ct = default)
        {
            List<Manufacturer> manufacturers = await service.GetManufacturersAsync(ct);

            return Ok(manufacturers.ToDto());
        }

        [HttpPut("update_from_cloud_api"), Authorize(Roles = RolesConstants.ADMIN)]
        public IActionResult UpdateManufacturersFromCloudApi(CancellationToken ct = default)
        {
            bool started = backgroundUpdateService.TryStart(
                nameof(UpdateManufacturersFromCloudApi),
                (provider, token) => provider.GetRequiredService<ManufacturerService>().UpdateManufacturersFromCloudApi(token));

            return this.ToBackgroundUpdateResponse(started);
        }

        [HttpPut("update_from_cloud_db"), Authorize(Roles = RolesConstants.ADMIN)]
        public IActionResult UpdateManufacturersFromCloudDb(CancellationToken ct)
        {
            bool started = backgroundUpdateService.TryStart(
                nameof(UpdateManufacturersFromCloudDb),
                (provider, token) => provider.GetRequiredService<ManufacturerService>().UpdateManufacturersFromCloudDb(token));

            return this.ToBackgroundUpdateResponse(started);
        }
    }
}
