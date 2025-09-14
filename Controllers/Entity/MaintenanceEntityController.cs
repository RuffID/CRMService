using CRMService.Interfaces.Repository;
using CRMService.Models.ConfigClass;
using CRMService.Models.Dto.Entity;
using CRMService.Models.Entity;
using CRMService.Service.Entity;
using CRMService.Service.Sync;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRMService.Controllers.Entity
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class MaintenanceEntityController(IUnitOfWork unitOfWork, EntitySyncService sync, MaintenanceEntityService service) : Controller
    {
        [HttpGet]
        public async Task<IActionResult> GetMaintenanceEntity([FromQuery] int id, CancellationToken ct)
        {
            MaintenanceEntity? maintenanceEntity = await unitOfWork.MaintenanceEntity.GetItemById(id, true, ct);

            if (maintenanceEntity == null)
                return NotFound();

            MaintenanceEntityDto dto = new()
            {
                Id = maintenanceEntity.Id,
                Name = maintenanceEntity.Name,
                Address = maintenanceEntity.Address,
                Active = maintenanceEntity.Active
            };

            return Ok(dto);
        }

        [HttpGet("list")]
        public async Task<IActionResult> GetMaintenanceEntities([FromQuery] int startIndex = 0, CancellationToken ct = default)
        {
            List<MaintenanceEntity> maintenanceEntities = await unitOfWork.MaintenanceEntity.GetItemsByPredicateAndSortById(predicate: me => me.Id >= startIndex, take: LimitConstants.LIMIT_FOR_RETRIEVING_ENTITIES_FROM_DB, asNoTracking: true, ct: ct);

            List<MaintenanceEntityDto> dtos = maintenanceEntities.Select(me =>  new MaintenanceEntityDto()
            {
                Id = me.Id,
                Name = me.Name,
                Address = me.Address,
                Active = me.Active
            }).ToList();

            return Ok(dtos);
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

        [HttpPut("update_from_cloud_api"), Authorize(Roles = RolesDefinitionConstants.ADMIN)]
        public async Task<IActionResult> UpdateMaintenanceEntitiesFromCloudApi([FromQuery] long startIndex = 0, CancellationToken ct = default)
        {
            await sync.RunExclusive(async () =>
            {
                await service.UpdateMaintenanceEntitiesFromCloudApi(startIndex, LimitConstants.LIMIT_FOR_RETRIEVING_ENTITIES_FROM_API, ct);
            });

            return NoContent();
        }

        [HttpPut("update_from_cloud_db"), Authorize(Roles = RolesDefinitionConstants.ADMIN)]
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
