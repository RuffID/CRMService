using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CRMService.Application.Service.OkdeskEntity;
using CRMService.Domain.Models.OkdeskEntity;
using CRMService.Domain.Models.Constants;
using Microsoft.EntityFrameworkCore;
using CRMService.Application.Abstractions.Database.Repository;
using CRMService.Application.Common.Mapping.OkdeskEntity;

namespace CRMService.Web.Controllers.OkdeskEntity
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class EquipmentController(IUnitOfWork unitOfWork, EquipmentService service) : Controller
    {
        [HttpGet]
        public async Task<IActionResult> GetEquipment([FromQuery] int id, CancellationToken ct)
        {
            Equipment? equipment = await unitOfWork.Equipment.GetItemByIdAsync(id, asNoTracking: true, include: e => e.Include(e => e.Parameters), ct: ct);

            if (equipment == null)
                return NotFound();

            return Ok(equipment.ToDto());
        }

        [HttpGet("by_maintenance_entity")]
        public async Task<IActionResult> GetEquipmentsByMaintenanceEntity([FromQuery] int maintenanceEntityId, CancellationToken ct)
        {
            List<Equipment> equipments = await unitOfWork.Equipment.GetItemsByPredicateAsync(predicate: e => e.MaintenanceEntitiesId == maintenanceEntityId, asNoTracking: true, include: me => me.Include(me => me.Parameters), ct: ct);

            return Ok(equipments.ToDto());
        }

        [HttpGet("by_company")]
        public async Task<IActionResult> GetEquipmentsByCompany([FromQuery] int companyId, CancellationToken ct)
        {
            List<Equipment> equipments = await unitOfWork.Equipment.GetItemsByPredicateAsync(predicate: e => e.CompanyId == companyId, asNoTracking: true, include: e => e.Include(e => e.Parameters), ct: ct);

            return Ok(equipments.ToDto());
        }

        [HttpPut]
        public async Task<IActionResult> UpdateEquipmentFromCloudApi([FromQuery] long equipmentId = 0, CancellationToken ct = default)
        {
            await service.UpdateEquipmentFromCloudApiAsync(equipmentId, ct);

            return NoContent();
        }

        [HttpPut("update_by_company")]
        public async Task<IActionResult> UpdateEquipmentsByCompanyFromCloudApi([FromQuery] long companyId = 0, CancellationToken ct = default)
        {
            await service.UpdateEquipmentsFromCloudApiAsnc(companyId: companyId, ct: ct);

            return NoContent();
        }

        [HttpPut("update_by_maintenance")]
        public async Task<IActionResult> UpdateEquipmentsByMaintenanceFromCloudApi([FromQuery] long maintenanceEntityId = 0, CancellationToken ct = default)
        {
            await service.UpdateEquipmentsFromCloudApiAsnc(maintenanceEntityId: maintenanceEntityId, ct: ct);

            return NoContent();
        }

        [HttpPut("update_from_cloud_api"), Authorize(Roles = RolesConstants.ADMIN)]
        public async Task<IActionResult> UpdateEquipmentsFromCloudApi(CancellationToken ct = default)
        {
            await service.UpdateEquipmentsFromCloudApiAsnc(ct: ct);

            return NoContent();
        }

        [HttpPut("update_from_cloud_db"), Authorize(Roles = RolesConstants.ADMIN)]
        public async Task<IActionResult> UpdateEquipmentsFromDBOkdesk(CancellationToken ct = default)
        {
            await service.UpdateEquipmentsFromCloudDbAsync(ct);

            return NoContent();
        }
    }
}