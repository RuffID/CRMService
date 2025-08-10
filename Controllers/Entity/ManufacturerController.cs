using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using CRMService.Models.ConfigClass;
using CRMService.Core;
using CRMService.Service.Entity;
using CRMService.Interfaces.Repository;
using CRMService.Dto.Entity;

namespace CRMService.Controllers.Entity
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ManufacturerController(IMapper mapper, IOptions<DatabaseSettings> dbSettings, IOptions<OkdeskSettings> okdSettings, IUnitOfWorkEntities unitOfWork, ManufacturerService service) : Controller
    {
        [HttpGet("list")]
        public async Task<IActionResult> GetManufacturers([FromQuery] int startIndex)
        {
            var manufacturers = mapper.Map<IEnumerable<ManufacturerDto>>(await unitOfWork.Manufacturer.GetItems(startIndex, dbSettings.Value.LimitForRetrievingEntitiesFromDb));

            if (manufacturers == null || !manufacturers.Any())
                return NotFound("Manufacturers not found.");

            return Ok(manufacturers);
        }
                
        [HttpPut("update_from_cloud_api"), Authorize(Roles = UserRole.ADMIN)]
        public async Task<IActionResult> UpdateManufacturersFromCloudApi([FromQuery] long startIndex = 0)
        {
            await service.UpdateManufacturersFromCloudApi(startIndex, okdSettings.Value.LimitForRetrievingEntitiesFromApi);

            return NoContent();
        }

        [HttpPut("update_from_cloud_db"), Authorize(Roles = UserRole.ADMIN)]
        public async Task<IActionResult> UpdateManufacturersFromCloudDb()
        {
            await service.UpdateManufacturersFromCloudDb();

            return NoContent();
        }
    }
}
