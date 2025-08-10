using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CRMService.Models.ConfigClass;
using Microsoft.Extensions.Options;
using CRMService.Core;
using CRMService.Service.Entity;
using CRMService.Interfaces.Repository;
using CRMService.Service.Sync;
using CRMService.Dto.Entity;

namespace CRMService.Controllers.Entity
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class EquipmentController(IUnitOfWorkEntities unitOfWork, IOptions<DatabaseSettings> dbSettings, EntitySyncService sync, IOptions<OkdeskSettings> okdSettings, EquipmentService service) : Controller
    {

        [HttpGet]
        public async Task<IActionResult> GetEquipment([FromQuery] int id)
        {
            EquipmentDto? equipment = await unitOfWork.Equipment.GetEquipmentById(id);

            if (equipment == null)
                return NotFound("Equipment not found.");

            equipment.Parameters = await unitOfWork.Parameter.GetParameterByEquipmentId(equipment.Id);

            return Ok(equipment);
        }

        [HttpGet("by_maintenance_entity")]
        public async Task<IActionResult> GetEquipmentsByMaintenanceEntity([FromQuery] int maintenanceEntityId)
        {
            IEnumerable<EquipmentDto>? equipments = await unitOfWork.Equipment.GetEquipmentsByMaintenanceEntity(maintenanceEntityId);

            if (equipments == null || !equipments.Any())
                return NotFound("Equipments not found.");

            foreach (var equipment in equipments)
                equipment.Parameters = await unitOfWork.Parameter.GetParameterByEquipmentId(equipment.Id);

            return Ok(equipments);
        }

        [HttpGet("by_company")]
        public async Task<IActionResult> GetEquipmentsByCompany([FromQuery] int companyId)
        {
            IEnumerable<EquipmentDto>? equipments = await unitOfWork.Equipment.GetEquipmentsByCompany(companyId);

            if (equipments == null || !equipments.Any())
                return NotFound("Equipments not found.");

            foreach (var equipment in equipments)
                equipment.Parameters = await unitOfWork.Parameter.GetParameterByEquipmentId(equipment.Id);

            return Ok(equipments);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateEquipmentFromCloudApi([FromQuery] long equipmentId = 0)
        {
            await sync.RunExclusive(async () =>
            {
                await service.UpdateEquipmentFromCloudApi(equipmentId);
            });

            return NoContent();
        }

        [HttpPut("update_by_company")]
        public async Task<IActionResult> UpdateEquipmentsByCompanyFromCloudApi([FromQuery] int startIndex = 0, [FromQuery] long id = 0)
        {
            await sync.RunExclusive(async () =>
            {
                await service.UpdateEquipmentsFromCloudApi(startIndex, okdSettings.Value.LimitForRetrievingEntitiesFromApi, companyId: id);
            });

            return NoContent();
        }

        [HttpPut("update_by_maintenance")]
        public async Task<IActionResult> UpdateEquipmentsByMaintenanceFromCloudApi([FromQuery] int startIndex = 0, [FromQuery] long id = 0)
        {
            await sync.RunExclusive(async () =>
            {
                await service.UpdateEquipmentsFromCloudApi(startIndex, okdSettings.Value.LimitForRetrievingEntitiesFromApi, maintenanceEntityId: id);
            });

            return NoContent();
        }

        [HttpPut("update_from_cloud_api"), Authorize(Roles = UserRole.ADMIN)]
        public async Task<IActionResult> UpdateEquipmentsFromCloudApi([FromQuery] int startIndex = 0)
        {
            await sync.RunExclusive(async () =>
            {
                await service.UpdateEquipmentsFromCloudApi(startIndex, okdSettings.Value.LimitForRetrievingEntitiesFromApi);
            });

            return NoContent();
        }

        [HttpPut("update_from_cloud_db"), Authorize(Roles = UserRole.ADMIN)]
        public async Task<IActionResult> UpdateEquipmentsFromDBOkdesk([FromQuery] int startIndex = 0)
        {
            await sync.RunExclusive(async () =>
            {
                await service.UpdateEquipmentsFromCloudDb(startIndex, dbSettings.Value.LimitForRetrievingEntitiesFromDb);
            });

            return NoContent();
        }
    }
}
