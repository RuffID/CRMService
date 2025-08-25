using AutoMapper;
using CRMService.Interfaces.Repository;
using CRMService.Models.Authorization;
using CRMService.Models.Dto.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRMService.Controllers.Authorization
{
    [Authorize]
    [ApiController]
    [Route("api/authorize/[controller]")]
    public class SessionController(IMapper mapper, IUnitOfWorkAuthorization unitOfWork) : Controller
    {

        [HttpGet("list"), Authorize(Roles = RolesDefinition.ADMIN)]
        public async Task<IActionResult> GetSessions([FromQuery] int startIndex = 0, [FromQuery] int endIndex = 100)
        {
            IEnumerable<SessionDto>? sessions = mapper.Map<IEnumerable<SessionDto>>(await unitOfWork.Session.GetItems(new Range(startIndex, endIndex)));

            if (sessions == null || !sessions.Any())
                return NotFound("Sessions not found.");

            return Ok(sessions);
        }

        [HttpGet, Authorize(Roles = RolesDefinition.ADMIN)]
        public async Task<IActionResult> GetSession([FromQuery] Guid id)
        {
            SessionDto? session = mapper.Map<SessionDto>(await unitOfWork.Session.GetItem(new() { Id = id }, false));

            if (session == null)
                return NotFound($"Session {id} not found.");

            return Ok(session);
        }

        [HttpDelete, Authorize(Roles = RolesDefinition.ADMIN)]
        public async Task<IActionResult> DeleteSession([FromQuery] Guid id)
        {
            Session? session = await unitOfWork.Session.GetItem(new() { Id = id });

            if (session == null)
                return BadRequest($"Session {id} not found.");

            unitOfWork.Session.Delete(session);
            await unitOfWork.SaveAsync();

            return NoContent();
        }

        [HttpDelete("all"), Authorize(Roles = RolesDefinition.ADMIN)]
        public async Task<IActionResult> DeleteSessions()
        {
            IEnumerable<Session>? sessions = await unitOfWork.Session.GetItems(new Range(0, await unitOfWork.Session.GetCountOfItems()));

            if (sessions == null || !sessions.Any())
                return BadRequest($"Sessions not found.");

            foreach (Session session in sessions)
                unitOfWork.Session.Delete(session);

            await unitOfWork.SaveAsync();

            return NoContent();
        }

        [HttpDelete("by-userid"), Authorize(Roles = RolesDefinition.ADMIN)]
        public async Task<IActionResult> DeleteSessionsByUserId([FromQuery] Guid id)
        {
            await unitOfWork.Session.DeleteByUserId(id);
            return NoContent();
        }
    }
}
