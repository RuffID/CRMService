using CRMService.Abstractions.Database.Repository;
using CRMService.Models.Constants;
using CRMService.Models.Dto.Mappers.OkdeskEntity;
using CRMService.Models.OkdeskEntity;
using CRMService.Service.OkdeskEntity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRMService.Controllers.OkdeskEntity
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class KindParameterController(IUnitOfWork unitOfWork, KindParameterService kindParameterService, KindParamService kindParamService) : Controller
    {
        [HttpGet("list")]
        public async Task<IActionResult> GetKindParameters(CancellationToken ct = default)
        {
            List<KindsParameter> kindParameters = await unitOfWork.KindParameter.GetItemsByPredicateAsync(asNoTracking: true, ct: ct);

            return Ok(kindParameters.ToDto());
        }

        [HttpPut("update_from_cloud_api"), Authorize(Roles = RolesConstants.ADMIN)]
        public async Task<IActionResult> UpdateKindParametersFromCloudApi(CancellationToken ct)
        {
            await kindParameterService.UpdateKindParametersFromCloudApi(ct);
            await kindParamService.UpsertConnectionsFromCloudDb(ct);

            return NoContent();
        }

        [HttpPut("update_from_cloud_db"), Authorize(Roles = RolesConstants.ADMIN)]
        public async Task<IActionResult> UpdateKindParametersFromCloudDb(CancellationToken ct)
        {
            await kindParameterService.UpdateKindParametersFromCloudDb(ct);

            return NoContent();
        }

        [HttpPut("update_connections_from_cloud_api"), Authorize(Roles = RolesConstants.ADMIN)]
        public async Task<IActionResult> UpdateConnectionsFromCloudApi(CancellationToken ct)
        {
            await kindParamService.UpsertConnectionsFromCloudDb(ct);

            return NoContent();
        }
    }
}