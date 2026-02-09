using CRMService.Abstractions.Database.Repository;
using CRMService.Models.Constants;
using CRMService.Models.Dto.Mappers.OkdeskEntity;
using CRMService.Models.OkdeskEntity;
using CRMService.Service.OkdeskEntity;
using CRMService.Service.Sync;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRMService.Controllers.OkdeskEntity
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class MaintenanceEntityController(IUnitOfWork unitOfWork, EntitySyncService sync, MaintenanceEntityService service) : Controller
    {
        [HttpGet]
        public async Task<IActionResult> GetMaintenanceEntity([FromQuery] int id, CancellationToken ct)
        {
            MaintenanceEntity? maintenanceEntity = await unitOfWork.MaintenanceEntity.GetItemByIdAsync(id, asNoTracking: true, ct: ct);

            if (maintenanceEntity == null)
                return NotFound();

            return Ok(maintenanceEntity.ToDto());
        }

        [HttpGet("list")]
        public async Task<IActionResult> GetMaintenanceEntities(CancellationToken ct = default)
        {
            List<MaintenanceEntity> maintenanceEntities = await unitOfWork.MaintenanceEntity.GetItemsByPredicateAsync(asNoTracking: true, ct: ct);

            return Ok(maintenanceEntities.ToDto());
        }

        [HttpPut("update_from_api")]
        public async Task<IActionResult> UpdateMaintenanceEntityFromCloudApi([FromQuery] int maintenanceEntityId, CancellationToken ct)
        {
            await sync.RunExclusive(async () =>
            {
                await service.UpdateMaintenanceEntityFromCloudApi(maintenanceEntityId, ct);
            });

            return NoContent();
        }

        [HttpPut("update_from_cloud_api"), Authorize(Roles = RolesConstants.ADMIN)]
        public async Task<IActionResult> UpdateMaintenanceEntitiesFromCloudApi(CancellationToken ct = default)
        {
            await sync.RunExclusive(async () =>
            {
                await service.UpdateMaintenanceEntitiesFromCloudApi(ct);
            });

            return NoContent();
        }

        [HttpPut("update_from_cloud_db"), Authorize(Roles = RolesConstants.ADMIN)]
        public async Task<IActionResult> UpdateMaintenanceEntitiesFromCloudDb(CancellationToken ct = default)
        {
            await sync.RunExclusive(async () =>
            {
                await service.UpdateMaintenanceEntitiesFromCloudDb(ct);
            });

            return NoContent();
        }
    }
}
