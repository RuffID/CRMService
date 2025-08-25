using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CRMService.Models.ConfigClass;
using Microsoft.Extensions.Options;
using AutoMapper;
using CRMService.Models.Entity;
using CRMService.Service.Entity;
using CRMService.Interfaces.Repository;
using CRMService.Service.Sync;
using CRMService.Models.Enum;
using CRMService.Models.Dto.Entity;

namespace CRMService.Controllers.Entity
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class IssueController(IMapper mapper, IOptions<DatabaseSettings> dbSettings, IOptions<OkdeskSettings> okdSettings, EntitySyncService sync, 
        IUnitOfWork unitOfWork, IssueService service) : Controller
    {
        [HttpGet("list")]
        public async Task<IActionResult> GetIssues([FromQuery] int startIndex = 0)
        {
            var issues = mapper.Map<IEnumerable<IssueDto>>(await unitOfWork.Issue.GetItems(startIndex, okdSettings.Value.LimitForRetrievingEntitiesFromApi));

            if (issues == null || !issues.Any())
                return NotFound();

            return Ok(issues);
        }

        [HttpGet]
        public async Task<IActionResult> GetIssue([FromQuery] int id)
        {
            var issueFromDB = mapper.Map<IssueDto>(await unitOfWork.Issue.GetItem(new Issue() { Id = id}, false));

            if (issueFromDB == null)
                return NotFound("Issue not found.");

            return Ok(issueFromDB);
        }        

        [HttpPut("update_from_cloud_api"), Authorize(Roles = nameof(UserRole.Admin))]
        public async Task<IActionResult> UpdateIssuesFromCloudAPI([FromQuery] DateTime dateFrom, [FromQuery] DateTime dateTo, [FromQuery] long startIndex = 0)
        {
            if(dateFrom > dateTo)
                return BadRequest("Start date is later than end date.");

            if (dateTo.Hour == 0 && dateTo.Minute == 0 && dateTo.Second == 0)
                dateTo = new(dateTo.Year, dateTo.Month, dateTo.Day, hour: 23, minute: 59, second: 59);

            await sync.RunExclusive(async () =>
            {
                await service.UpdateIssuesFromCloudApi(dateFrom, dateTo, startIndex, limit: 50, nameof(IssueController));
            });

            return NoContent();
        }

        [HttpPut("update_from_cloud_db"), Authorize(Roles = nameof(UserRole.Admin))]
        public async Task<IActionResult> UpdateIssuesFromCloudDb([FromQuery] DateTime dateFrom, [FromQuery] DateTime dateTo, [FromQuery] int startIndex = 0)
        {
            if (dateFrom > dateTo)
                return BadRequest("Start date is later than end date.");

            if (dateTo.Hour == 0 && dateTo.Minute == 0 && dateTo.Second == 0)
                dateTo = new(dateTo.Year, dateTo.Month, dateTo.Day, hour: 23, minute: 59, second: 59);

            await sync.RunExclusive(async () =>
            {
                await service.UpdateIssuesFromCloudDb(dateFrom, dateTo, startIndex, dbSettings.Value.LimitForRetrievingEntitiesFromDb);
            });

            return NoContent();
        }
    }
}