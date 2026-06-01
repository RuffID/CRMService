using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CRMService.Application.Service.OkdeskEntity;
using CRMService.Domain.Models.OkdeskEntity;
using CRMService.Domain.Models.Constants;
using Microsoft.EntityFrameworkCore;
using CRMService.Application.Abstractions.Database.Repository;
using CRMService.Application.Common.Mapping.OkdeskEntity;
using CRMService.Web.Service.BackgroundServices;

namespace CRMService.Web.Controllers.OkdeskEntity
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class EquipmentController(IUnitOfWork unitOfWork, EquipmentService service, BackgroundUpdateService backgroundUpdateService) : Controller
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
        public IActionResult UpdateEquipmentsByCompanyFromCloudApi([FromQuery] long companyId = 0, CancellationToken ct = default)
        {
            bool started = backgroundUpdateService.TryStart(
                $"{nameof(UpdateEquipmentsByCompanyFromCloudApi)}:{companyId}",
                (provider, token) => provider.GetRequiredService<EquipmentService>().UpdateEquipmentsFromCloudApiAsnc(companyId: companyId, ct: token));

            return this.ToBackgroundUpdateResponse(started);
        }

        [HttpPut("update_by_maintenance")]
        public IActionResult UpdateEquipmentsByMaintenanceFromCloudApi([FromQuery] long maintenanceEntityId = 0, CancellationToken ct = default)
        {
            bool started = backgroundUpdateService.TryStart(
                $"{nameof(UpdateEquipmentsByMaintenanceFromCloudApi)}:{maintenanceEntityId}",
                (provider, token) => provider.GetRequiredService<EquipmentService>().UpdateEquipmentsFromCloudApiAsnc(maintenanceEntityId: maintenanceEntityId, ct: token));

            return this.ToBackgroundUpdateResponse(started);
        }

        [HttpPut("update_from_cloud_api"), Authorize(Roles = RolesConstants.ADMIN)]
        public IActionResult UpdateEquipmentsFromCloudApi(CancellationToken ct = default)
        {
            bool started = backgroundUpdateService.TryStart(
                nameof(UpdateEquipmentsFromCloudApi),
                (provider, token) => provider.GetRequiredService<EquipmentService>().UpdateEquipmentsFromCloudApiAsnc(ct: token));

            return this.ToBackgroundUpdateResponse(started);
        }

        [HttpPut("update_from_cloud_db"), Authorize(Roles = RolesConstants.ADMIN)]
        public IActionResult UpdateEquipmentsFromDBOkdesk(CancellationToken ct = default)
        {
            bool started = backgroundUpdateService.TryStart(
                nameof(UpdateEquipmentsFromDBOkdesk),
                (provider, token) => provider.GetRequiredService<EquipmentService>().UpdateEquipmentsFromCloudDbAsync(token));

            return this.ToBackgroundUpdateResponse(started);
        }
    }
}
