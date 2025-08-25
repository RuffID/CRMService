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
    public class KindController(IMapper mapper, IOptions<DatabaseSettings> dbSettings, IUnitOfWork unitOfWork, KindService service) : Controller
    {
        [HttpGet("list")]
        public async Task<IActionResult> GetKinds([FromQuery] int startIndex = 0)
        {
            var kinds = mapper.Map<ICollection<KindDto>>(await unitOfWork.Kind.GetItems(startIndex, dbSettings.Value.LimitForRetrievingEntitiesFromDb));

            if (kinds == null || kinds.Count <= 0)
                return NotFound("Kinds not found.");

            return Ok(kinds);
        }

        [HttpPut("update_from_cloud_api"), Authorize(Roles = nameof(UserRole.Admin))]
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
