using CRMService.Interfaces.Repository;
using CRMService.Models.Authorization;
using CRMService.Models.ConfigClass;
using CRMService.Models.Dto.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRMService.Controllers.Authorization
{
    [Authorize]
    [ApiController]
    [Route("api/authorize/[controller]")]
    public class SessionController(IUnitOfWork unitOfWork) : Controller
    {

        [HttpGet("list"), Authorize(Roles = RolesDefinitionConstants.ADMIN)]
        public async Task<IActionResult> GetSessions([FromQuery] int skip = 0, [FromQuery] int limit = LimitConstants.LIMIT_FOR_RETRIEVING_ENTITIES_FROM_DB, CancellationToken ct = default)
        {
            List<Session> sessions = await unitOfWork.Session.GetItemsByPredicate(skip: skip, take: limit, asNoTracking: true, ct: ct);

            List<SessionDto> dtos = sessions.Select(s => new SessionDto()
            {
                Id = s.Id,
                UserId = s.UserId,
                RefreshToken = s.RefreshToken,
                ExpirationRefreshToken = s.ExpirationRefreshToken

            }).ToList();

            return Ok(dtos);
        }

        [HttpGet, Authorize(Roles = RolesDefinitionConstants.ADMIN)]
        public async Task<IActionResult> GetSession([FromQuery] Guid id, CancellationToken ct)
        {
            Session? session = await unitOfWork.Session.GetItemById(id, asNoTracking: true, ct);

            if (session == null)
                return NotFound($"Session {id} not found.");

            SessionDto dto = new()
            {
                Id = session.Id,
                UserId = session.UserId,
                RefreshToken = session.RefreshToken,
                ExpirationRefreshToken = session.ExpirationRefreshToken
            };

            return Ok(dto);
        }

        [HttpDelete, Authorize(Roles = RolesDefinitionConstants.ADMIN)]
        public async Task<IActionResult> DeleteSession([FromQuery] Guid id, CancellationToken ct)
        {
            Session? session = await unitOfWork.Session.GetItemById(id, asNoTracking: true, ct);

            if (session == null)
                return BadRequest($"Session {id} not found.");

            unitOfWork.Session.Delete(session);
            await unitOfWork.SaveAsync(ct);

            return NoContent();
        }
    }
}
