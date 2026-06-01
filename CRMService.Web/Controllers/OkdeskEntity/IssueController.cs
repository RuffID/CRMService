using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CRMService.Domain.Models.OkdeskEntity;
using CRMService.Application.Service.OkdeskEntity;
using CRMService.Domain.Models.Constants;
using CRMService.Application.Abstractions.Database.Repository;
using CRMService.Application.Common.Mapping.OkdeskEntity;
using CRMService.Web.Service.BackgroundServices;

namespace CRMService.Web.Controllers.OkdeskEntity
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class IssueController(IUnitOfWork unitOfWork, BackgroundUpdateService backgroundUpdateService) : Controller
    {
        [HttpGet("list")]
        public async Task<IActionResult> GetIssues([FromQuery] int startIndex = 0, CancellationToken ct = default)
        {
            List<Issue> issues = await unitOfWork.Issue.GetItemsByPredicateAsync(predicate: i => i.Id >= startIndex, take: LimitConstants.LIMIT_FOR_RETRIEVING_ENTITIES_FROM_DB, asNoTracking: true, ct: ct);

            return Ok(issues.ToDto());
        }

        [HttpGet]
        public async Task<IActionResult> GetIssue([FromQuery] int id, CancellationToken ct)
        {
            Issue? issue = await unitOfWork.Issue.GetItemByIdAsync(id: id, asNoTracking: true, ct: ct);

            if (issue == null)
                return NotFound();

            return Ok(issue.ToDto());
        }

        [HttpPut("update_from_cloud_api"), Authorize(Roles = RolesConstants.ADMIN)]
        public IActionResult UpdateIssuesFromCloudAPI([FromQuery] DateTime dateFrom, [FromQuery] DateTime dateTo, [FromQuery] long startIndex = 0, CancellationToken ct = default)
        {
            if (dateFrom > dateTo)
                return BadRequest("Start date is later than end date.");

            if (dateTo.Hour == 0 && dateTo.Minute == 0 && dateTo.Second == 0)
                dateTo = new(dateTo.Year, dateTo.Month, dateTo.Day, hour: 23, minute: 59, second: 59);

            bool started = backgroundUpdateService.TryStart(
                $"{nameof(UpdateIssuesFromCloudAPI)}:{dateFrom:O}:{dateTo:O}:{startIndex}",
                (provider, token) => provider.GetRequiredService<IssueService>().UpdateIssuesFromCloudApiAsync(dateFrom, dateTo, startIndex, limit: LimitConstants.LIMIT_FOR_RETRIEVING_ENTITIES_FROM_API, nameof(IssueController), token));

            return this.ToBackgroundUpdateResponse(started);
        }

        [HttpPut("update_from_cloud_db"), Authorize(Roles = RolesConstants.ADMIN)]
        public IActionResult UpdateIssuesFromCloudDb([FromQuery] DateTime dateFrom, [FromQuery] DateTime dateTo, [FromQuery] int startIndex = 0, CancellationToken ct = default)
        {
            if (dateFrom > dateTo)
                return BadRequest("Start date is later than end date.");

            if (dateTo.Hour == 0 && dateTo.Minute == 0 && dateTo.Second == 0)
                dateTo = new(dateTo.Year, dateTo.Month, dateTo.Day, hour: 23, minute: 59, second: 59);

            dateFrom = ConvertToUtc(dateFrom);
            dateTo = ConvertToUtc(dateTo);

            bool started = backgroundUpdateService.TryStart(
                $"{nameof(UpdateIssuesFromCloudDb)}:{dateFrom:O}:{dateTo:O}:{startIndex}",
                (provider, token) => provider.GetRequiredService<IssueService>().UpdateIssuesFromCloudDbAsync(dateFrom, dateTo, startIndex, LimitConstants.LIMIT_FOR_RETRIEVING_ENTITIES_FROM_DB, nameof(IssueController), token));

            return this.ToBackgroundUpdateResponse(started);
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
