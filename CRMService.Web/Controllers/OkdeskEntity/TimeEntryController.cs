using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CRMService.Application.Service.OkdeskEntity;
using CRMService.Domain.Models.Constants;

namespace CRMService.Web.Controllers.OkdeskEntity
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class TimeEntryController(TimeEntryService service) : Controller
    {
        [HttpPut("update_from_cloud_db"), Authorize(Roles = RolesConstants.ADMIN)]
        public async Task<IActionResult> UpdateTimeEntriesFromCloudDb([FromQuery] DateTime dateFrom, [FromQuery] DateTime dateTo, CancellationToken ct = default)
        {
            if (dateFrom > dateTo)
                return BadRequest("Start date is later than end date.");

            if (dateTo.Hour == 0 || dateTo.Minute == 0 || dateTo.Second == 0)
                dateTo = DateTime.SpecifyKind(new DateTime(dateTo.Year, dateTo.Month, dateTo.Day, hour: 23, minute: 59, second: 59), dateTo.Kind);

            dateFrom = ConvertToUtc(dateFrom);
            dateTo = ConvertToUtc(dateTo);

            await service.UpdateTimeEntriesFromCloudDb(dateFrom, dateTo, ct);

            return NoContent();
        }

        [HttpPut("update_from_cloud_api"), Authorize(Roles = RolesConstants.ADMIN)]
        public async Task<IActionResult> UpdateTimeEntriesFromCloudApi([FromQuery] int issueId, CancellationToken ct = default)
        {
            await service.UpdateTimeEntriesFromCloudApi(issueId, ct);

            return NoContent();
        }

        private static DateTime ConvertToUtc(DateTime dateTime)
        {
            // Обрабатывает локальное время как UTC-границу запроса к PostgreSQL timestamptz.
            return dateTime.Kind switch
            {
                DateTimeKind.Utc => dateTime,
                DateTimeKind.Local => dateTime.ToUniversalTime(),
                DateTimeKind.Unspecified => TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(dateTime, DateTimeKind.Local)),
                _ => throw new ArgumentOutOfRangeException(nameof(dateTime), dateTime.Kind, "Unsupported DateTime kind.")
            };
        }
    }
}
