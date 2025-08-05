using CRMService.Core;
using CRMService.Service.Sync;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRMService.Controllers.Hosted
{
    [Authorize, Authorize(Roles = UserRole.ADMIN)]
    [Route("api/crm/[controller]")]
    [ApiController]
    public class UpdateDirectoriesController(Service.Hosted.UpdateDirectoriesService service, EntitySyncService sync) : Controller
    {
        [HttpPost]
        public async Task<IActionResult> RunUpdate()
        {
            await sync.RunExclusive(service.RunUpdateDirectories);

            return NoContent();
        }
    }
}
