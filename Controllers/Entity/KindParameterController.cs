using AutoMapper;
using CRMService.Interfaces.Repository;
using CRMService.Models.ConfigClass;
using CRMService.Models.Dto.Entity;
using CRMService.Models.Entity;
using CRMService.Service.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRMService.Controllers.Entity
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class KindParameterController(IMapper mapper, IUnitOfWork unitOfWork, KindParameterService kindParameterService, KindParamService kindParamService) : Controller
    {
        [HttpGet("list")]
        public async Task<IActionResult> GetKindParameters([FromQuery] int startIndex = 0, CancellationToken ct = default)
        {
            List<KindsParameter> kindParameters = await unitOfWork.KindParameter.GetItemsByPredicateAndSortById(predicate: p => p.Id >= startIndex, take: LimitConstants.LIMIT_FOR_RETRIEVING_ENTITIES_FROM_DB, asNoTracking: true, ct: ct);

            return Ok(mapper.Map<List<KindParameterDto>>(kindParameters));
        }

        [HttpPut("update_from_cloud_api"), Authorize(Roles = RolesDefinitionConstants.ADMIN)]
        public async Task<IActionResult> UpdateKindParametersFromCloudApi(CancellationToken ct)
        {
            await kindParameterService.UpdateKindParametersFromCloudApi(ct);
            await kindParamService.UpdateConnectionsFromCloudDb(ct);

            return NoContent();
        }

        [HttpPut("update_from_cloud_db"), Authorize(Roles = RolesDefinitionConstants.ADMIN)]
        public async Task<IActionResult> UpdateKindParametersFromCloudDb(CancellationToken ct)
        {
            await kindParameterService.UpdateKindParametersFromCloudDb(ct);

            return NoContent();
        }

        [HttpPut("update_connections_from_cloud_api"), Authorize(Roles = RolesDefinitionConstants.ADMIN)]
        public async Task<IActionResult> UpdateConnectionsFromCloudApi(CancellationToken ct)
        {
            await kindParamService.UpdateConnectionsFromCloudDb(ct);

            return NoContent();
        }
    }
}
