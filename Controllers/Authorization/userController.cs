using AutoMapper;
using CRMService.Interfaces.Repository;
using CRMService.Models.Authorization;
using CRMService.Models.ConfigClass;
using CRMService.Models.Dto.Authorization;
using CRMService.Models.Request;
using CRMService.Service.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRMService.Controllers.Authorization
{
    [Authorize]
    [ApiController]
    [Route("api/authorize/[controller]")]
    public class UserController(IUnitOfWorkAuthorization unitOfWork, IMapper mapper, Hasher hasher) : Controller
    {
        [HttpGet("list"), Authorize(Roles = RolesDefinition.ADMIN)]
        public async Task<IActionResult> GetUsers([FromQuery] int startIndex = 0, [FromQuery] int limit = LimitConstants.LIMIT_FOR_RETRIEVING_ENTITIES_FROM_DB, CancellationToken ct = default)
        {
            List<User> users = await unitOfWork.User.GetItemsByPredicate(skip: startIndex, take: limit, asNoTracking: true, ct: ct);

            return Ok(mapper.Map<List<UserDto>>(users));
        }

        [HttpGet, Authorize(Roles = RolesDefinition.ADMIN)]
        public async Task<IActionResult> GetUser([FromBody] UserRequest userCreate, CancellationToken ct)
        {
            User? user = await unitOfWork.User.GetItemByPredicate(predicate: u => u.Login == userCreate.Login, asNoTracking: true, ct: ct);

            if (user == null)
                return NotFound("User not found.");

            return Ok(mapper.Map<UserDto>(user));
        }

        [HttpPut, Authorize(Roles = RolesDefinition.ADMIN)]
        public async Task<IActionResult> UpdateUser([FromBody] UserRequest userUpdate, CancellationToken ct)
        {
            User? user = await unitOfWork.User.GetItemByPredicate(predicate: u => u.Login == userUpdate.Login, asNoTracking: false, ct: ct);

            if (user == null)
                return NotFound($"User not found.");

            user.Password = hasher.Hash(user.Password);
            user.Active = user.Active;

            if (user.Roles != null && user.Roles.Count != 0)
            {
                user.Roles = userUpdate.Roles.Select(r => new CrmRole()
                {
                    Id = r.Id,
                    Name = r.Name
                }).ToList();
            }

            await unitOfWork.SaveAsync();

            return NoContent();
        }        
    }
}