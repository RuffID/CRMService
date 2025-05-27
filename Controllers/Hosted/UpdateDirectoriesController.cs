using CRMService.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRMService.Controllers.Hosted
{
    [Authorize, Authorize(Roles = UserRole.ADMIN)]
    [Route("api/crm/[controller]")]
    [ApiController]
    public class UpdateDirectoriesController(Service.Hosted.UpdateDirectoriesService service) : Controller
    {
        [HttpPost]
        public async Task<IActionResult> RunUpdate()
        {
            await service.RunUpdateDirectories();

            return NoContent();
        }
    }
}
