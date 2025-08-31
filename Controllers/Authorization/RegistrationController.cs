using AutoMapper;
using CRMService.Interfaces.Repository;
using CRMService.Models.Authorization;
using CRMService.Models.Request;
using CRMService.Service.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRMService.Controllers.Authorization
{
    [Authorize]
    [ApiController]
    [Route("api/authorize/[controller]")]
    public class RegistrationController(IMapper mapper, IUnitOfWork unitOfWork, Hasher hash) : Controller
    {
        [HttpPost, Authorize(Roles = RolesDefinition.ADMIN)]
        public async Task<IActionResult> Registration([FromBody] UserRequest userCreate, CancellationToken ct)
        {
            User? existingUser = await unitOfWork.User.GetItemByPredicate(predicate: u => u.Login == userCreate.Login, asNoTracking: false, ct);

            if (existingUser != null)            
                return Conflict($"[Method:{nameof(Registration)}] User with login {userCreate.Login} already exists.");

            List<CrmRole> roles = await unitOfWork.CrmRole.GetItemsByCollection(mapper.Map<User>(userCreate).Roles, false, ct);

            User user = new()
            {
                Login = userCreate.Login,
                Roles = roles,
                Password = hash.Hash(userCreate.Password!),
                Active = true
            };

            unitOfWork.User.Create(user);
            await unitOfWork.SaveAsync(ct);

            return NoContent();
        }
    }
}