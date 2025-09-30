using CRMService.Interfaces.Repository;
using CRMService.Models.Authorization;
using CRMService.Models.Constants;
using CRMService.Models.Dto.Authorization;
using CRMService.Models.Dto.Mappers.Authorize;
using CRMService.Service.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRMService.Controllers.Authorization
{
    [Authorize]
    [ApiController]
    [Route("api/authorize/[controller]")]
    public class UserController(IUnitOfWork unitOfWork) : Controller
    {
        private readonly Hasher hash = new();

        [HttpGet("list"), Authorize(Roles = RolesConstants.ADMIN)]
        public async Task<IActionResult> GetUsers([FromQuery] int startIndex = 0, [FromQuery] int limit = LimitConstants.LIMIT_FOR_RETRIEVING_ENTITIES_FROM_DB, CancellationToken ct = default)
        {
            List<User> users = await unitOfWork.User.GetItemsByPredicate(skip: startIndex, take: limit, asNoTracking: true, ct: ct);

            return Ok(users.ToDto());
        }

        [HttpGet, Authorize(Roles = RolesConstants.ADMIN)]
        public async Task<IActionResult> GetUser([FromBody] string login, CancellationToken ct)
        {
            User? user = await unitOfWork.User.GetItemByPredicate(predicate: u => u.Login == login, asNoTracking: true, ct: ct);

            if (user == null)
                return NotFound("User not found.");

            return Ok(user.ToDto());
        }

        [HttpPut, Authorize(Roles = RolesConstants.ADMIN)]
        public async Task<IActionResult> UpdateUser([FromBody] UserDto userUpdate, CancellationToken ct)
        {
            User? user = await unitOfWork.User.GetItemByPredicate(predicate: u => u.Login == userUpdate.Login, asNoTracking: false, ct: ct);

            if (user == null)
                return NotFound($"User not found.");

            user.Active = userUpdate.Active;

            user.Roles = userUpdate.Roles.Select(r => new CrmRole()
            {
                Id = r.Id,
                Name = r.Name
            }).ToList();

            await unitOfWork.SaveAsync(ct);

            return NoContent();
        }        
    }
}