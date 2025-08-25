using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using CRMService.Models.ConfigClass;
using CRMService.Service.Entity;
using CRMService.Interfaces.Repository;
using CRMService.Models.Enum;
using CRMService.Models.Dto.Entity;

namespace CRMService.Controllers.Entity
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class KindParameterController(IMapper mapper, IUnitOfWork unitOfWork, IOptions<DatabaseSettings> dbSettings, KindParameterService kindParameterService, KindParamService kindParamService) : Controller
    {

        [HttpGet("list")]
        public async Task<IActionResult> GetKindParameters([FromQuery] int startIndex)
        {
            var kindParameters = mapper.Map<ICollection<KindParameterDto>>(await unitOfWork.KindParameter.GetItems(startIndex, dbSettings.Value.LimitForRetrievingEntitiesFromDb));

            if (kindParameters == null || kindParameters.Count <= 0)
                return NotFound();

            return Ok(kindParameters);
        }

        [HttpPut("update_from_cloud_api"), Authorize(Roles = nameof(UserRole.Admin))]
        public async Task<IActionResult> UpdateKindParametersFromCloudApi()
        {
            await kindParameterService.UpdateKindParametersFromCloudApi();
            await kindParamService.UpdateConnectionsFromCloudDb();

            return NoContent();
        }

        [HttpPut("update_from_cloud_db"), Authorize(Roles = nameof(UserRole.Admin))]
        public async Task<IActionResult> UpdateKindParametersFromCloudDb()
        {
            await kindParameterService.UpdateKindParametersFromCloudDb();

            return NoContent();
        }

        [HttpPut("update_connections_from_cloud_api"), Authorize(Roles = nameof(UserRole.Admin))]
        public async Task<IActionResult> UpdateConnectionsFromCloudApi()
        {
            await kindParamService.UpdateConnectionsFromCloudDb();

            return NoContent();
        }
    }
}
