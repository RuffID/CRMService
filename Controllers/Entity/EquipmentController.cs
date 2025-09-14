using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CRMService.Models.ConfigClass;
using CRMService.Service.Entity;
using CRMService.Interfaces.Repository;
using CRMService.Service.Sync;
using CRMService.Models.Dto.Entity;
using CRMService.Models.Entity;
using AutoMapper;

namespace CRMService.Controllers.Entity
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class EquipmentController(IUnitOfWork unitOfWork, IMapper mapper, EntitySyncService sync, EquipmentService service) : Controller
    {
        [HttpGet]
        public async Task<IActionResult> GetEquipment([FromQuery] int id, CancellationToken ct)
        {
            Equipment? equipment = await unitOfWork.Equipment.GetItemById(id, asNoTracking: true, ct, e => e.Parameters);

            if (equipment == null)
                return NotFound($"Equipment by id {id} - not found.");

            return Ok(mapper.Map<EquipmentDto>(equipment));
        }

        [HttpGet("by_maintenance_entity")]
        public async Task<IActionResult> GetEquipmentsByMaintenanceEntity([FromQuery] int maintenanceEntityId, CancellationToken ct)
        {
            List<Equipment> equipments = await unitOfWork.Equipment.GetItemsByPredicateAndSortById(predicate: e => e.MaintenanceEntitiesId == maintenanceEntityId, asNoTracking: true, ct: ct, includes: e => e.Parameters);

            return Ok(equipments);
        }

        [HttpGet("by_company")]
        public async Task<IActionResult> GetEquipmentsByCompany([FromQuery] int companyId, CancellationToken ct)
        {
            List<Equipment> equipments = await unitOfWork.Equipment.GetItemsByPredicateAndSortById(predicate: e => e.CompanyId == companyId, asNoTracking: true, ct: ct, includes: e => e.Parameters);

            return Ok(equipments);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateEquipmentFromCloudApi([FromQuery] long equipmentId = 0, CancellationToken ct = default)
        {
            await sync.RunExclusive(async () =>
            {
                await service.UpdateEquipmentFromCloudApi(equipmentId, ct);
            });

            return NoContent();
        }

        [HttpPut("update_by_company")]
        public async Task<IActionResult> UpdateEquipmentsByCompanyFromCloudApi([FromQuery] int startIndex = 0, [FromQuery] long id = 0)
        {
            await sync.RunExclusive(async () =>
            {
                await service.UpdateEquipmentsFromCloudApi(startIndex, LimitConstants.LIMIT_FOR_RETRIEVING_ENTITIES_FROM_API, companyId: id);
            });

            return NoContent();
        }

        [HttpPut("update_by_maintenance")]
        public async Task<IActionResult> UpdateEquipmentsByMaintenanceFromCloudApi([FromQuery] int startIndex = 0, [FromQuery] long id = 0, CancellationToken ct = default)
        {
            await sync.RunExclusive(async () =>
            {
                await service.UpdateEquipmentsFromCloudApi(startIndex, LimitConstants.LIMIT_FOR_RETRIEVING_ENTITIES_FROM_API, maintenanceEntityId: id, ct: ct);
            });

            return NoContent();
        }

        [HttpPut("update_from_cloud_api"), Authorize(Roles = RolesDefinitionConstants.ADMIN)]
        public async Task<IActionResult> UpdateEquipmentsFromCloudApi([FromQuery] int startIndex = 0, CancellationToken ct = default)
        {
            await sync.RunExclusive(async () =>
            {
                await service.UpdateEquipmentsFromCloudApi(startIndex, LimitConstants.LIMIT_FOR_RETRIEVING_ENTITIES_FROM_API, ct: ct);
            });

            return NoContent();
        }

        [HttpPut("update_from_cloud_db"), Authorize(Roles = RolesDefinitionConstants.ADMIN)]
        public async Task<IActionResult> UpdateEquipmentsFromDBOkdesk([FromQuery] int startIndex = 0, CancellationToken ct = default)
        {
            await sync.RunExclusive(async () =>
            {
                await service.UpdateEquipmentsFromCloudDb(startIndex, LimitConstants.LIMIT_FOR_RETRIEVING_ENTITIES_FROM_API, ct);
            });

            return NoContent();
        }
    }
}
