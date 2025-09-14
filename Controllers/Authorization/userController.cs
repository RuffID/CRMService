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
    public class UserController(IUnitOfWork unitOfWork) : Controller
    {
        private readonly Hasher hash = new();

        [HttpGet("list"), Authorize(Roles = RolesDefinitionConstants.ADMIN)]
        public async Task<IActionResult> GetUsers([FromQuery] int startIndex = 0, [FromQuery] int limit = LimitConstants.LIMIT_FOR_RETRIEVING_ENTITIES_FROM_DB, CancellationToken ct = default)
        {
            List<User> users = await unitOfWork.User.GetItemsByPredicate(skip: startIndex, take: limit, asNoTracking: true, ct: ct);

            List<UserDto> dtos = users.Select(u => new UserDto()
            {
                Id = u.Id,
                Login = u.Login,
                Active = u.Active,
                Roles = u.Roles.Select(r => new RoleDto()
                {
                    Id = r.Id,
                    Name = r.Name
                }).ToList()
            }).ToList();

            return Ok(dtos);
        }

        [HttpGet, Authorize(Roles = RolesDefinitionConstants.ADMIN)]
        public async Task<IActionResult> GetUser([FromBody] UserRequest userGet, CancellationToken ct)
        {
            User? user = await unitOfWork.User.GetItemByPredicate(predicate: u => u.Login == userGet.Login, asNoTracking: true, ct: ct);

            if (user == null)
                return NotFound("User not found.");

            UserDto dto = new ()
            {
                Id = user.Id,
                Login = user.Login,
                Active = user.Active,
                Roles = user.Roles.Select(r => new RoleDto()
                {
                    Id = r.Id,
                    Name = r.Name
                }).ToList()
            };

            return Ok(dto);
        }

        [HttpPut, Authorize(Roles = RolesDefinitionConstants.ADMIN)]
        public async Task<IActionResult> UpdateUser([FromBody] UserRequest userUpdate, CancellationToken ct)
        {
            User? user = await unitOfWork.User.GetItemByPredicate(predicate: u => u.Login == userUpdate.Login, asNoTracking: false, ct: ct);

            if (user == null)
                return NotFound($"User not found.");

            user.Password = hash.Hash(user.Password);
            user.Active = user.Active;

            if (user.Roles != null && user.Roles.Count != 0)
            {
                user.Roles = userUpdate.Roles.Select(r => new CrmRole()
                {
                    Id = r.Id,
                    Name = r.Name
                }).ToList();
            }

            await unitOfWork.SaveAsync(ct);

            return NoContent();
        }        
    }
}