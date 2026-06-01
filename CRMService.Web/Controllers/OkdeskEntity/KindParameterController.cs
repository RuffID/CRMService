using CRMService.Application.Abstractions.Database.Repository;
using CRMService.Domain.Models.Constants;
using CRMService.Domain.Models.OkdeskEntity;
using CRMService.Application.Service.OkdeskEntity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CRMService.Application.Common.Mapping.OkdeskEntity;
using CRMService.Web.Service.BackgroundServices;

namespace CRMService.Web.Controllers.OkdeskEntity
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class KindParameterController(IUnitOfWork unitOfWork, BackgroundUpdateService backgroundUpdateService) : Controller
    {
        [HttpGet("list")]
        public async Task<IActionResult> GetKindParameters(CancellationToken ct = default)
        {
            List<KindsParameter> kindParameters = await unitOfWork.KindParameter.GetItemsByPredicateAsync(asNoTracking: true, ct: ct);

            return Ok(kindParameters.ToDto());
        }

        [HttpPut("update_from_cloud_api"), Authorize(Roles = RolesConstants.ADMIN)]
        public IActionResult UpdateKindParametersFromCloudApi(CancellationToken ct)
        {
            bool started = backgroundUpdateService.TryStart(
                nameof(UpdateKindParametersFromCloudApi),
                async (provider, token) =>
                {
                    await provider.GetRequiredService<KindParameterService>().UpdateKindParametersFromCloudApi(token);
                    await provider.GetRequiredService<KindParamService>().UpsertConnectionsFromCloudDb(token);
                });

            return this.ToBackgroundUpdateResponse(started);
        }

        [HttpPut("update_from_cloud_db"), Authorize(Roles = RolesConstants.ADMIN)]
        public IActionResult UpdateKindParametersFromCloudDb(CancellationToken ct)
        {
            bool started = backgroundUpdateService.TryStart(
                nameof(UpdateKindParametersFromCloudDb),
                (provider, token) => provider.GetRequiredService<KindParameterService>().UpdateKindParametersFromCloudDb(token));

            return this.ToBackgroundUpdateResponse(started);
        }

        [HttpPut("update_connections_from_cloud_api"), Authorize(Roles = RolesConstants.ADMIN)]
        public IActionResult UpdateConnectionsFromCloudApi(CancellationToken ct)
        {
            bool started = backgroundUpdateService.TryStart(
                nameof(UpdateConnectionsFromCloudApi),
                (provider, token) => provider.GetRequiredService<KindParamService>().UpsertConnectionsFromCloudDb(token));

            return this.ToBackgroundUpdateResponse(started);
        }
    }
}
