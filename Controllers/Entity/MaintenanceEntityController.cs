using AutoMapper;
using CRMService.Interfaces.Repository;
using CRMService.Models.ConfigClass;
using CRMService.Models.Dto.Entity;
using CRMService.Models.Entity;
using CRMService.Models.Enum;
using CRMService.Service.Entity;
using CRMService.Service.Sync;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRMService.Controllers.Entity
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class MaintenanceEntityController(IMapper mapper, IUnitOfWork unitOfWork, EntitySyncService sync, MaintenanceEntityService service) : Controller
    {
        [HttpGet]
        public async Task<IActionResult> GetMaintenanceEntity([FromQuery] int id, CancellationToken ct)
        {
            MaintenanceEntity? maintenanceEntityFromDB = await unitOfWork.MaintenanceEntity.GetItemById(id, true, ct);

            if (maintenanceEntityFromDB == null)
                return NotFound();

            return Ok(mapper.Map<MaintenanceEntityDto>(maintenanceEntityFromDB));
        }

        [HttpGet("list")]
        public async Task<IActionResult> GetMaintenanceEntities([FromQuery] int startIndex = 0, CancellationToken ct = default)
        {
            List<MaintenanceEntity> maintenanceEntities = await unitOfWork.MaintenanceEntity.GetItemsByPredicateAndSortById(predicate: me => me.Id >= startIndex, take: LimitConstants.LIMIT_FOR_RETRIEVING_ENTITIES_FROM_DB, asNoTracking: true, ct: ct);

            return Ok(mapper.Map<List<MaintenanceEntityDto>>(maintenanceEntities));
        }

        [HttpPut("update_maintenance_entity_from_cloud_api")]
        public async Task<IActionResult> UpdateMaintenanceEntityFromCloudApi([FromQuery] int maintenanceEntityId, CancellationToken ct)
        {
            if (maintenanceEntityId == 0)
                return BadRequest("Wrong id.");

            await sync.RunExclusive(async () =>
            {
                await service.UpdateMaintenanceEntityFromCloudApi(maintenanceEntityId, ct);
            });

            return NoContent();
        }

        [HttpPut("update_from_cloud_api"), Authorize(Roles = nameof(UserRole.Admin))]
        public async Task<IActionResult> UpdateMaintenanceEntitiesFromCloudApi([FromQuery] long startIndex = 0, CancellationToken ct = default)
        {
            await sync.RunExclusive(async () =>
            {
                await service.UpdateMaintenanceEntitiesFromCloudApi(startIndex, LimitConstants.LIMIT_FOR_RETRIEVING_ENTITIES_FROM_API, ct);
            });

            return NoContent();
        }

        [HttpPut("update_from_cloud_db"), Authorize(Roles = nameof(UserRole.Admin))]
        public async Task<IActionResult> UpdateMaintenanceEntitiesFromCloudDb([FromQuery] long startIndex = 0, CancellationToken ct = default)
        {
            await sync.RunExclusive(async () =>
            {
               await service.UpdateMaintenanceEntitiesFromCloudDb(startIndex, LimitConstants.LIMIT_FOR_RETRIEVING_ENTITIES_FROM_DB, ct);
            });

            return NoContent();
        }
    }
}
