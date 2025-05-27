using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using CRMService.Dto;
using Microsoft.AspNetCore.Authorization;
using CRMService.Models.ConfigClass;
using Microsoft.Extensions.Options;
using CRMService.Core;
using CRMService.Service.Entity;
using CRMService.Interfaces.Repository;

namespace CRMService.Controllers.Entity
{
    [Authorize]
    [Route("api/crm/[controller]")]
    [ApiController]
    public class MaintenanceEntityController(IMapper mapper, IOptions<DatabaseSettings> dbSettings, IOptions<OkdeskSettings> okdSettings, IUnitOfWorkEntities unitOfWork, MaintenanceEntityService service) : Controller
    {

        [HttpGet]
        public async Task<IActionResult> GetMaintenanceEntity([FromQuery] int id)
        {
            var maintenanceEntityFromDB = mapper.Map<MaintenanceEntityDto>(await unitOfWork.MaintenanceEntity.GetMaintenanceEntityById(id));

            if (maintenanceEntityFromDB == null)
                return NotFound("Maintenance entity not found.");

            return Ok(maintenanceEntityFromDB);
        }

        [HttpGet("list")]
        public async Task<IActionResult> GetMaintenanceEntities([FromQuery] int startIndex = 0)
        {
            IEnumerable<MaintenanceEntityDto>? maintenanceEntities = mapper.Map<IEnumerable<MaintenanceEntityDto>>(await unitOfWork.MaintenanceEntity.GetItems(startIndex, dbSettings.Value.LimitForRetrievingEntitiesFromDb));

            if (maintenanceEntities == null || !maintenanceEntities.Any())
                return NotFound("Maintenance entities not found.");

            return Ok(maintenanceEntities);
        }

        [HttpPut("update_maintenance_entity_from_cloud_api")]
        public async Task<IActionResult> UpdateMaintenanceEntityFromCloudApi([FromQuery] int maintenanceEntityId)
        {
            if (maintenanceEntityId == 0)
                BadRequest("Maintenance entity id not set.");

            await service.UpdateMaintenanceEntityFromCloudApi(maintenanceEntityId);

            return NoContent();
        }

        [HttpPut("update_from_cloud_api"), Authorize(Roles = UserRole.ADMIN)]
        public async Task<IActionResult> UpdateMaintenanceEntitiesFromCloudApi([FromQuery] long startIndex = 0)
        {
            await service.UpdateMaintenanceEntitiesFromCloudApi(startIndex, okdSettings.Value.LimitForRetrievingEntitiesFromApi);

            return NoContent();
        }

        [HttpPut("update_from_cloud_db"), Authorize(Roles = UserRole.ADMIN)]
        public async Task<IActionResult> UpdateMaintenanceEntitiesFromCloudDb()
        {
            await service.UpdateMaintenanceEntitiesFromCloudDb();

            return NoContent();
        }
    }
}
