using CRMService.Domain.Models.Constants;
using CRMService.Application.Service.Sync;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CRMService.Application.Service.Hosted;

namespace CRMService.Web.Controllers.Hosted
{
    [Authorize, Authorize(Roles = RolesConstants.ADMIN)]
    [Route("api/[controller]")]
    [ApiController]
    public class UpdateDirectoriesController(UpdateDirectoriesService service, EntitySyncService sync) : Controller
    {
        [HttpPost]
        public async Task<IActionResult> RunUpdate()
        {
            await sync.RunExclusive(async () => await service.RunUpdateDirectories());

            return NoContent();
        }
    }
}




