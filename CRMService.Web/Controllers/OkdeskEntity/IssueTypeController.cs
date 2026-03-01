using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CRMService.Domain.Models.OkdeskEntity;
using CRMService.Application.Service.OkdeskEntity;
using CRMService.Domain.Models.Constants;
using CRMService.Application.Abstractions.Database.Repository;
using CRMService.Application.Common.Mapping.OkdeskEntity;

namespace CRMService.Web.Controllers.OkdeskEntity
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class IssueTypeController(IUnitOfWork unitOfWork, IssueTypeService service) : Controller
    {
        [HttpGet("list")]
        public async Task<IActionResult> GetIssueTypes(CancellationToken ct)
        {
            return JsonResultMapper.ToJsonResult(await service.GetTypes(ct));
        }

        [HttpGet]
        public async Task<IActionResult> GetIssueType([FromQuery] string code, CancellationToken ct)
        {
            IssueType? type = await unitOfWork.IssueType.GetItemByPredicateAsync(it => it.Code == code, true, ct: ct);

            if (type == null)
                return NotFound();

            return Ok(type.ToDto());
        }

        [HttpPut("update_from_cloud_api"), Authorize(Roles = RolesConstants.ADMIN)]
        public async Task<IActionResult> UpdateIssueTypesFromCloudApi(CancellationToken ct)
        {
            await service.UpdateIssueTypesFromCloudApi(ct);

            return NoContent();
        }

        [HttpPut("update_from_cloud_db"), Authorize(Roles = RolesConstants.ADMIN)]
        public async Task<IActionResult> UpdatetIssueTypesFromCloudDb(CancellationToken ct)
        {
            await service.UpdateIssueTypesFromCloudDb(ct);

            return NoContent();
        }
    }
}