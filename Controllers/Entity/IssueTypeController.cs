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
    [Route("api/crm/[controller]")]
    [ApiController]
    public class IssueTypeController(IMapper mapper, IOptions<DatabaseSettings> dbSettings, IUnitOfWorkEntities unitOfWork, IssueTypeService service) : Controller
    {
        [HttpGet("list")]
        public async Task<IActionResult> GetIssueTypes([FromQuery] int startIndex)
        {
            var types = mapper.Map<IEnumerable<TaskTypeDto>>(await unitOfWork.IssueType.GetItems(startIndex, dbSettings.Value.LimitForRetrievingEntitiesFromDb));

            if (types == null || !types.Any())
                return NotFound();

            return Ok(types);
        }

        [HttpGet]
        public async Task<IActionResult> GetIssueType([FromQuery] string code)
        {
            var type = mapper.Map<TaskTypeDto>(await unitOfWork.IssueType.GetItem(new IssueType() { Code = code }, false));

            if (type == null)
                return NotFound();

            return Ok(type);
        }

        [HttpPut("update_from_cloud_api"), Authorize(Roles = UserRole.ADMIN)]
        public async Task<IActionResult> UpdateIssueTypesFromCloudApi()
        {
            await service.UpdateIssueTypesFromCloudApi();

            return NoContent();
        }

        [HttpPut("update_from_cloud_db"), Authorize(Roles = UserRole.ADMIN)]
        public async Task<IActionResult> UpdatetIssueTypesFromCloudDb()
        {
            await service.UpdateIssueTypesFromCloudDb();

            return NoContent();
        }
    }
}