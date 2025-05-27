using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CRMService.Core;
using CRMService.Service.Entity;

namespace CRMService.Controllers.Entity
{
    [Authorize]
    [Route("api/crm/[controller]")]
    [ApiController]
    public class TimeEntryController(TimeEntryService service) : Controller
    {
        [HttpPut("update_from_cloud_db"), Authorize(Roles = UserRole.ADMIN)]
        public async Task<IActionResult> UpdateTimeEntriesFromCloudDb([FromQuery] DateTime dateFrom, [FromQuery] DateTime dateTo, [FromQuery] long timeEntryId = 0)
        {
            if (dateFrom > dateTo)
                return BadRequest("Start date is later than end date.");

            if (dateTo.Hour == 0 || dateTo.Minute == 0 || dateTo.Second == 0) 
                dateTo = new(dateTo.Year, dateTo.Month, dateTo.Day, hour: 23, minute: 59, second: 59);

            await service.UpdateTimeEntriesFromCloudDb(dateFrom, dateTo, timeEntryId);

            return NoContent();
        }

        [HttpPut("update_from_cloud_api"), Authorize(Roles = UserRole.ADMIN)]
        public async Task<IActionResult> UpdateTimeEntriesFromCloudApi([FromQuery] int issueId)
        {
            await service.UpdateTimeEntriesFromCloudApi(issueId);

            return NoContent();
        }
    }
}