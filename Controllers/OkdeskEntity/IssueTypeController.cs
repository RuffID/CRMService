using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CRMService.Models.OkdeskEntity;
using CRMService.Service.OkdeskEntity;
using CRMService.Models.Constants;
using CRMService.Models.Dto.Mappers.OkdeskEntity;
using CRMService.Abstractions.Database.Repository;

namespace CRMService.Controllers.OkdeskEntity
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class IssueTypeController(IUnitOfWork unitOfWork, IssueTypeService service) : Controller
    {
        [HttpGet("list")]
        public async Task<IActionResult> GetIssueTypes(CancellationToken ct)
        {
            return Ok((await service.GetTypes(ct)).Data);
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