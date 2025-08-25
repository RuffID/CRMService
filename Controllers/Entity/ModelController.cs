using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CRMService.Models.ConfigClass;
using Microsoft.Extensions.Options;
using CRMService.Service.Entity;
using CRMService.Interfaces.Repository;
using CRMService.Models.Enum;
using CRMService.Models.Dto.Entity;

namespace CRMService.Controllers.Entity
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ModelController(IMapper mapper, IOptions<DatabaseSettings> dbSettings, IOptions<OkdeskSettings> okdSettings, IUnitOfWork unitOfWork, ModelService service) : Controller
    {

        [HttpGet("list")]
        public async Task<IActionResult> GetModels([FromQuery] int startIndex)
        {
            var models = mapper.Map<IEnumerable<ModelDto>>(await unitOfWork.Model.GetItems(startIndex, dbSettings.Value.LimitForRetrievingEntitiesFromDb));

            if (models == null || !models.Any())
                return NotFound("Models not found.");

            return Ok(models);
        }
                

        [HttpPut("update_from_cloud_api"), Authorize(Roles = nameof(UserRole.Admin))]
        public async Task<IActionResult> UpdateModelsFromCloudApi([FromQuery] long startIndex = 0)
        {
            await service.UpdateModelsFromCloudApi(startIndex, okdSettings.Value.LimitForRetrievingEntitiesFromApi);

            return NoContent();
        }

        [HttpPut("update_from_cloud_db"), Authorize(Roles = nameof(UserRole.Admin))]
        public async Task<IActionResult> UpdateModelsFromCloudDb()
        {
            await service.UpdateModelsFromCloudDb();

            return NoContent();
        }
    }
}
