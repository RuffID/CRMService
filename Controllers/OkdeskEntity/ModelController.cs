using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CRMService.Service.OkdeskEntity;
using CRMService.Models.OkdeskEntity;
using CRMService.Models.Constants;
using CRMService.Models.Dto.Mappers.OkdeskEntity;
using CRMService.Abstractions.Database.Repository;

namespace CRMService.Controllers.OkdeskEntity
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ModelController(IUnitOfWork unitOfWork, ModelService service) : Controller
    {
        [HttpGet("list")]
        public async Task<IActionResult> GetModels(CancellationToken ct = default)
        {
            List<Model> models = await unitOfWork.Model.GetItemsByPredicateAsync(asNoTracking: true, ct: ct);

            return Ok(models.ToDto());
        }


        [HttpPut("update_from_cloud_api"), Authorize(Roles = RolesConstants.ADMIN)]
        public async Task<IActionResult> UpdateModelsFromCloudApi(CancellationToken ct = default)
        {
            await service.UpdateModelsFromCloudApi(ct);

            return NoContent();
        }

        [HttpPut("update_from_cloud_db"), Authorize(Roles = RolesConstants.ADMIN)]
        public async Task<IActionResult> UpdateModelsFromCloudDb(CancellationToken ct)
        {
            await service.UpdateModelsFromCloudDb(ct);

            return NoContent();
        }
    }
}
