using CRMService.Models.Constants;
using CRMService.Service.Sync;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRMService.Controllers.Hosted
{
    [Authorize, Authorize(Roles = RolesConstants.ADMIN)]
    [Route("api/[controller]")]
    [ApiController]
    public class UpdateDirectoriesController(Service.Hosted.UpdateDirectoriesService service, EntitySyncService sync) : Controller
    {
        [HttpPost]
        public async Task<IActionResult> RunUpdate()
        {
            await sync.RunExclusive(async () => await service.RunUpdateDirectories());

            return NoContent();
        }
    }
}