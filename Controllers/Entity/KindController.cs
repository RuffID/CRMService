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
    public class KindController(IMapper mapper, IOptions<DatabaseSettings> dbSettings, IUnitOfWorkEntities unitOfWork, KindService service) : Controller
    {
        [HttpGet("list")]
        public async Task<IActionResult> GetKinds([FromQuery] int startIndex = 0)
        {
            var kinds = mapper.Map<ICollection<KindDto>>(await unitOfWork.Kind.GetItems(startIndex, dbSettings.Value.LimitForRetrievingEntitiesFromDb));

            if (kinds == null || kinds.Count <= 0)
                return NotFound("Kinds not found.");

            return Ok(kinds);
        }

        [HttpPut("update_from_cloud_api"), Authorize(Roles = UserRole.ADMIN)]
        public async Task<IActionResult> UpdateKindsFromCloudApi([FromQuery] int startIndex = 0)
        {
            await service.UpdateKindsFromCloudApi(startIndex, dbSettings.Value.LimitForRetrievingEntitiesFromDb);

            return NoContent();
        }

        [HttpPut("update_from_cloud_db")]
        public async Task<IActionResult> UpdateKindsFromCloudDb()
        {
            await service.UpdateKindsFromCloudDb();

            return NoContent();
        }
    }
}
