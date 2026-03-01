using CRMService.Domain.Models.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CRMService.Application.Service.Hosted;

namespace CRMService.Web.Controllers.Hosted
{
    [Authorize, Authorize(Roles = RolesConstants.ADMIN)]
    [Route("api/[controller]")]
    [ApiController]
    public class UpdateDirectoriesController(UpdateDirectoriesService service) : Controller
    {
        [HttpPost]
        public async Task<IActionResult> RunUpdate()
        {
            await service.RunUpdateDirectories();

            return NoContent();
        }
    }
}




