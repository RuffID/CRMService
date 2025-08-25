using CRMService.Models.Server;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRMService.Controllers.Server
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ServerstatusController(ServerData data) : Controller
    {

        [HttpGet, AllowAnonymous]
        public IActionResult ServerStatus()
        {
            TimeSpan upTime = DateTime.UtcNow - data.ServerStartingTime;
            string upTimeString = $"Days: {upTime.Days}, Hours: {upTime.Hours}, Minutes: {upTime.Minutes}";
            data.ServerUpTime = upTimeString;
            return Ok(data);
        }
    }
}