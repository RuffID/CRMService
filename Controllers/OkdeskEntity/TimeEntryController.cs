using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CRMService.Service.OkdeskEntity;
using CRMService.Models.Constants;

namespace CRMService.Controllers.OkdeskEntity
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class TimeEntryController(TimeEntryService service) : Controller
    {
        [HttpPut("update_from_cloud_db"), Authorize(Roles = RolesConstants.ADMIN)]
        public async Task<IActionResult> UpdateTimeEntriesFromCloudDb([FromQuery] DateTime dateFrom, [FromQuery] DateTime dateTo, [FromQuery] long startIndex = 0, CancellationToken ct = default)
        {
            if (dateFrom > dateTo)
                return BadRequest("Start date is later than end date.");

            if (dateTo.Hour == 0 || dateTo.Minute == 0 || dateTo.Second == 0) 
                dateTo = new(dateTo.Year, dateTo.Month, dateTo.Day, hour: 23, minute: 59, second: 59);

            await service.UpdateTimeEntriesFromCloudDb(dateFrom, dateTo, startIndex, LimitConstants.LIMIT_FOR_RETRIEVING_ENTITIES_FROM_DB, ct);

            return NoContent();
        }

        [HttpPut("update_from_cloud_api"), Authorize(Roles = RolesConstants.ADMIN)]
        public async Task<IActionResult> UpdateTimeEntriesFromCloudApi([FromQuery] int issueId, CancellationToken ct)
        {
            await service.UpdateTimeEntriesFromCloudApi(issueId, ct);

            return NoContent();
        }
    }
}