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
    public class MaintenanceEntityController(MaintenanceEntityService service, BackgroundUpdateService backgroundUpdateService) : Controller
    {
        [HttpGet]
        public async Task<IActionResult> GetMaintenanceEntity([FromQuery] int id, CancellationToken ct)
        {
            MaintenanceEntity? maintenanceEntity = await service.GetMaintenanceEntityAsync(id, ct);

            if (maintenanceEntity == null)
                return NotFound();

            return Ok(maintenanceEntity.ToDto());
        }

        [HttpGet("list")]
        public async Task<IActionResult> GetMaintenanceEntities(CancellationToken ct = default)
        {
            List<MaintenanceEntity> maintenanceEntities = await service.GetMaintenanceEntitiesAsync(ct);

            return Ok(maintenanceEntities.ToDto());
        }

        [HttpPut("update_from_api")]
        public async Task<IActionResult> UpdateMaintenanceEntityFromCloudApi([FromQuery] int maintenanceEntityId, CancellationToken ct)
        {
            await service.UpdateMaintenanceEntityFromCloudApi(maintenanceEntityId, ct);

            return NoContent();
        }

        [HttpPut("update_from_cloud_api"), Authorize(Roles = RolesConstants.ADMIN)]
        public IActionResult UpdateMaintenanceEntitiesFromCloudApi(CancellationToken ct = default)
        {
            bool started = backgroundUpdateService.TryStart(
                nameof(UpdateMaintenanceEntitiesFromCloudApi),
                (provider, token) => provider.GetRequiredService<MaintenanceEntityService>().UpdateMaintenanceEntitiesFromCloudApi(token));

            return this.ToBackgroundUpdateResponse(started);
        }

        [HttpPut("update_from_cloud_db"), Authorize(Roles = RolesConstants.ADMIN)]
        public IActionResult UpdateMaintenanceEntitiesFromCloudDb(CancellationToken ct = default)
        {
            bool started = backgroundUpdateService.TryStart(
                nameof(UpdateMaintenanceEntitiesFromCloudDb),
                (provider, token) => provider.GetRequiredService<MaintenanceEntityService>().UpdateMaintenanceEntitiesFromCloudDb(token));

            return this.ToBackgroundUpdateResponse(started);
        }
    }
}
