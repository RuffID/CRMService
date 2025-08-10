using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CRMService.Models.ConfigClass;
using Microsoft.Extensions.Options;
using CRMService.Core;
using CRMService.Models.Entity;
using CRMService.Service.Entity;
using CRMService.Interfaces.Repository;
using CRMService.Dto.Entity;

namespace CRMService.Controllers.Entity
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class IssueStatusController(IMapper mapper, IOptions<OkdeskSettings> okdSettings, IUnitOfWorkEntities unitOfWork, IssueStatusService service) : Controller
    {
        [HttpGet("list")]
        public async Task<IActionResult> GetIssueStatuses([FromQuery] int startIndex)
        {
            var statuses = mapper.Map<IEnumerable<StatusDto>>(await unitOfWork.IssueStatus.GetItems(startIndex, okdSettings.Value.LimitForRetrievingEntitiesFromApi));

            if (statuses == null || !statuses.Any())
                return NotFound();

            return Ok(statuses);
        }

        [HttpGet]
        public async Task<IActionResult> GetIssueStatus([FromQuery] string code)
        {
            var status = mapper.Map<StatusDto>(await unitOfWork.IssueStatus.GetItem(new IssueStatus() { Code = code }, false));

            if (status == null)
                return NotFound();

            return Ok(status);
        }

        [HttpPut("update_from_cloud_api"), Authorize(Roles = UserRole.ADMIN)]
        public async Task<IActionResult> UpdateIssueStatusesFromCloudApi()
        {
            await service.UpdateIssueStatusesFromCloudApi();

            return NoContent();
        }

        [HttpPut("update_from_cloud_db"), Authorize(Roles = UserRole.ADMIN)]
        public async Task<IActionResult> UpdateIssueStatusesFromCloudDb()
        {
            await service.UpdateIssueStatusesFromCloudDb();

            return NoContent();
        }
    }
}