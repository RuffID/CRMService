using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CRMService.Service.Entity;
using CRMService.Interfaces.Repository;
using CRMService.Models.Enum;
using CRMService.Models.Dto.Entity;
using CRMService.Models.ConfigClass;
using CRMService.Models.Entity;

namespace CRMService.Controllers.Entity
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ModelController(IMapper mapper, IUnitOfWork unitOfWork, ModelService service) : Controller
    {
        [HttpGet("list")]
        public async Task<IActionResult> GetModels([FromQuery] int startIndex = 0, CancellationToken ct = default)
        {
            List<Model> models = await unitOfWork.Model.GetItemsByPredicateAndSortById(predicate: m => m.Id >= startIndex, take: LimitConstants.LIMIT_FOR_RETRIEVING_ENTITIES_FROM_DB, asNoTracking: true, ct: ct);

            return Ok(mapper.Map<List<ModelDto>>(models));
        }
                

        [HttpPut("update_from_cloud_api"), Authorize(Roles = nameof(UserRole.Admin))]
        public async Task<IActionResult> UpdateModelsFromCloudApi([FromQuery] long startIndex = 0, CancellationToken ct = default)
        {
            await service.UpdateModelsFromCloudApi(startIndex, LimitConstants.LIMIT_FOR_RETRIEVING_ENTITIES_FROM_API, ct);

            return NoContent();
        }

        [HttpPut("update_from_cloud_db"), Authorize(Roles = nameof(UserRole.Admin))]
        public async Task<IActionResult> UpdateModelsFromCloudDb(CancellationToken ct)
        {
            await service.UpdateModelsFromCloudDb(ct);

            return NoContent();
        }
    }
}
