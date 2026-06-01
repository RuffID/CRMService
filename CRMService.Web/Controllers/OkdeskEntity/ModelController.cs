using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CRMService.Application.Service.OkdeskEntity;
using CRMService.Domain.Models.OkdeskEntity;
using CRMService.Domain.Models.Constants;
using CRMService.Application.Abstractions.Database.Repository;
using CRMService.Application.Common.Mapping.OkdeskEntity;
using CRMService.Web.Service.BackgroundServices;

namespace CRMService.Web.Controllers.OkdeskEntity
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ModelController(IUnitOfWork unitOfWork, BackgroundUpdateService backgroundUpdateService) : Controller
    {
        [HttpGet("list")]
        public async Task<IActionResult> GetModels(CancellationToken ct = default)
        {
            List<Model> models = await unitOfWork.Model.GetItemsByPredicateAsync(asNoTracking: true, ct: ct);

            return Ok(models.ToDto());
        }


        [HttpPut("update_from_cloud_api"), Authorize(Roles = RolesConstants.ADMIN)]
        public IActionResult UpdateModelsFromCloudApi(CancellationToken ct = default)
        {
            bool started = backgroundUpdateService.TryStart(
                nameof(UpdateModelsFromCloudApi),
                (provider, token) => provider.GetRequiredService<ModelService>().UpdateModelsFromCloudApi(token));

            return this.ToBackgroundUpdateResponse(started);
        }

        [HttpPut("update_from_cloud_db"), Authorize(Roles = RolesConstants.ADMIN)]
        public IActionResult UpdateModelsFromCloudDb(CancellationToken ct)
        {
            bool started = backgroundUpdateService.TryStart(
                nameof(UpdateModelsFromCloudDb),
                (provider, token) => provider.GetRequiredService<ModelService>().UpdateModelsFromCloudDb(token));

            return this.ToBackgroundUpdateResponse(started);
        }
    }
}
