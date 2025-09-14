using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CRMService.Models.ConfigClass;
using CRMService.Models.Entity;
using CRMService.Service.Entity;
using CRMService.Interfaces.Repository;
using CRMService.Service.Sync;
using CRMService.Models.Dto.Entity;

namespace CRMService.Controllers.Entity
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class IssueController(EntitySyncService sync, 
        IUnitOfWork unitOfWork, IssueService service) : Controller
    {
        [HttpGet("list")]
        public async Task<IActionResult> GetIssues([FromQuery] int startIndex = 0, CancellationToken ct = default)
        {
            List<Issue> issues = await unitOfWork.Issue.GetItemsByPredicateAndSortById(predicate: i => i.Id >= startIndex, take: LimitConstants.LIMIT_FOR_RETRIEVING_ENTITIES_FROM_DB, asNoTracking: true, ct: ct);

            List<IssueDto> dtos = issues.Select(i => new IssueDto()
            {
                Id = i.Id,
                AssigneeId = i.AssigneeId,
                AuthorId = i.AuthorId,
                Title = i.Title,
                EmployeesUpdatedAt = i.EmployeesUpdatedAt,
                CreatedAt = i.CreatedAt,
                CompletedAt = i.CompletedAt,
                DeadlineAt = i.DeadlineAt,
                DelayTo = i.DelayTo,
                StatusId = i.StatusId,
                PriorityId = i.PriorityId,
                TypeId = i.TypeId,
                CompanyId = i.CompanyId,
                ServiceObjectId = i.ServiceObjectId

            }).ToList();

            return Ok(dtos);
        }

        [HttpGet]
        public async Task<IActionResult> GetIssue([FromQuery] int id, CancellationToken ct)
        {
            Issue? issue = await unitOfWork.Issue.GetItemById(id, false, ct);

            if (issue == null)
                return NotFound();

            IssueDto dto = new ()
            {
                Id = issue.Id,
                AssigneeId = issue.AssigneeId,
                AuthorId = issue.AuthorId,
                Title = issue.Title,
                EmployeesUpdatedAt = issue.EmployeesUpdatedAt,
                CreatedAt = issue.CreatedAt,
                CompletedAt = issue.CompletedAt,
                DeadlineAt = issue.DeadlineAt,
                DelayTo = issue.DelayTo,
                StatusId = issue.StatusId,
                PriorityId = issue.PriorityId,
                TypeId = issue.TypeId,
                CompanyId = issue.CompanyId,
                ServiceObjectId = issue.ServiceObjectId
            };

            return Ok(dto);
        }        

        [HttpPut("update_from_cloud_api"), Authorize(Roles = RolesDefinitionConstants.ADMIN)]
        public async Task<IActionResult> UpdateIssuesFromCloudAPI([FromQuery] DateTime dateFrom, [FromQuery] DateTime dateTo, [FromQuery] long startIndex = 0, CancellationToken ct = default)
        {
            if(dateFrom > dateTo)
                return BadRequest("Start date is later than end date.");

            if (dateTo.Hour == 0 && dateTo.Minute == 0 && dateTo.Second == 0)
                dateTo = new(dateTo.Year, dateTo.Month, dateTo.Day, hour: 23, minute: 59, second: 59);

            await sync.RunExclusive(async () =>
            {
                await service.UpdateIssuesFromCloudApi(dateFrom, dateTo, startIndex, limit: LimitConstants.LIMIT_FOR_RETRIEVING_ENTITIES_FROM_API, nameof(IssueController), ct);
            });

            return NoContent();
        }

        [HttpPut("update_from_cloud_db"), Authorize(Roles = RolesDefinitionConstants.ADMIN)]
        public async Task<IActionResult> UpdateIssuesFromCloudDb([FromQuery] DateTime dateFrom, [FromQuery] DateTime dateTo, [FromQuery] int startIndex = 0, CancellationToken ct = default)
        {
            if (dateFrom > dateTo)
                return BadRequest("Start date is later than end date.");

            if (dateTo.Hour == 0 && dateTo.Minute == 0 && dateTo.Second == 0)
                dateTo = new(dateTo.Year, dateTo.Month, dateTo.Day, hour: 23, minute: 59, second: 59);

            await sync.RunExclusive(async () =>
            {
                await service.UpdateIssuesFromCloudDb(dateFrom, dateTo, startIndex, LimitConstants.LIMIT_FOR_RETRIEVING_ENTITIES_FROM_DB, nameof(IssueController), ct);
            });

            return NoContent();
        }
    }
}