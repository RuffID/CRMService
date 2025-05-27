using CRMService.Core;
using CRMService.Models.Server;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace CRMService.Controllers.Server
{
    [Authorize]
    [ApiController]
    [Route("api/crm/[controller]")]
    public class ServerstatusController(IHostApplicationLifetime appLifeTime, ILoggerFactory logger, ServerInfo immutable) : Controller
    {
        private readonly ILogger<ServerstatusController> _logger = logger.CreateLogger<ServerstatusController>();

        [HttpGet, AllowAnonymous]
        public IActionResult ServerStatus()
        {
            ServerData data = new()
            {
                ServerName = "CRMService",
                ServerStartingTime = immutable.ServerStartingTime.ToString("dd-MM-yyyy HH:mm")
            };

            TimeSpan upTime = DateTime.Now - immutable.ServerStartingTime;
            string upTimeString = $"Days: {upTime.Days}, Hours: {upTime.Hours}, Minutes: {upTime.Minutes}";
            data.ServerUpTime = upTimeString;
            return Ok(data);
        }

        [HttpGet("restart"), Authorize(Roles = UserRole.ADMIN)]
        public async Task<int> StopServiceAsync()
        {
            try
            {
                appLifeTime.StopApplication();
                var process = Process.GetCurrentProcess().MainModule;
                string? _currentProcess;
                if (process != null)
                    _currentProcess = Path.GetFullPath(process.FileName);
                else return 1;

                if (!string.IsNullOrEmpty(_currentProcess))
                    Process.Start(_currentProcess);
            }
            catch (Exception ex) 
            {
                _logger.LogError(ex, "Error while restart service.");
                return 1;
            }            

            return await Task.FromResult(0);
        }

    }

}
