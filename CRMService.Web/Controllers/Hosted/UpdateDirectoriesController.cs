using CRMService.Domain.Models.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CRMService.Application.Service.Hosted;
using CRMService.Web.Service.BackgroundServices;

namespace CRMService.Web.Controllers.Hosted
{
    [Authorize, Authorize(Roles = RolesConstants.ADMIN)]
    [Route("api/[controller]")]
    [ApiController]
    public class UpdateDirectoriesController(BackgroundUpdateService backgroundUpdateService) : Controller
    {
        [HttpPost]
        public IActionResult RunUpdate(CancellationToken ct)
        {
            bool started = backgroundUpdateService.TryStart(
                nameof(RunUpdate),
                (provider, token) => provider.GetRequiredService<UpdateDirectoriesService>().RunUpdateDirectories(token));

            return this.ToBackgroundUpdateResponse(started);
        }
    }
}
