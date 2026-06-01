using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CRMService.Application.Service.OkdeskEntity;
using CRMService.Domain.Models.OkdeskEntity;
using CRMService.Domain.Models.Constants;
using CRMService.Application.Abstractions.Database.Repository;
using CRMService.Application.Common.Mapping.OkdeskEntity;
using CRMService.Web.Service.BackgroundServices;

namespace CRMService.Web.Controllers.OkdeskEntity
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class RoleController(IUnitOfWork unitOfWork, BackgroundUpdateService backgroundUpdateService) : Controller
    {
        [HttpGet("list")]
        public async Task<IActionResult> GetRoles(CancellationToken ct = default)
        {
            List<OkdeskRole> roles = await unitOfWork.OkdeskRole.GetItemsByPredicateAsync(asNoTracking: true, ct: ct);

            return Ok(roles.ToDto());
        }

        [HttpPut("update_from_cloud_api"), Authorize(Roles = RolesConstants.ADMIN)]
        public IActionResult UpdateRolesFromCloudApi(CancellationToken ct)
        {
            bool started = backgroundUpdateService.TryStart(
                nameof(UpdateRolesFromCloudApi),
                (provider, token) => provider.GetRequiredService<RoleService>().UpdateRolesFromCloudApi(token));

            return this.ToBackgroundUpdateResponse(started);
        }

        [HttpPut("update_connections_with_employees_from_cloud_api"), Authorize(Roles = RolesConstants.ADMIN)]
        public IActionResult UpdateEmployeeRoleConnectionsFromCloudApi(CancellationToken ct)
        {
            bool started = backgroundUpdateService.TryStart(
                nameof(UpdateEmployeeRoleConnectionsFromCloudApi),
                (provider, token) => provider.GetRequiredService<RoleService>().UpsertEmployeeRoleConnectionsFromApi(token));

            return this.ToBackgroundUpdateResponse(started);
        }
    }
}
