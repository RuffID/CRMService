/*using CRMService.Interfaces.Repository;
using CRMService.Models.Authorization;
using CRMService.Models.Constants;
using CRMService.Models.Request;
using CRMService.Service.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRMService.Controllers.Authorization
{
    [Authorize]
    [ApiController]
    [Route("api/authorize/[controller]")]
    public class RegistrationController(IUnitOfWork unitOfWork) : Controller
    {
        private readonly Hasher hash = new();

        [HttpPost, Authorize(Roles = RolesConstants.ADMIN)]
        public async Task<IActionResult> Registration([FromBody] UserRequest userCreate, CancellationToken ct)
        {
            User? existingUser = await unitOfWork.User.GetItemByPredicate(predicate: u => u.Login == userCreate.Login, asNoTracking: true, ct);

            if (existingUser != null)
                return Conflict($"[Method:{nameof(Registration)}] User: {userCreate.Login} - already exists.");

            List<CrmRole> userCreateRoles = userCreate.Roles.Select(r => new CrmRole()
            {
                Id = r.Id,
                Name = r.Name
            }).ToList();

            List<CrmRole> roles = await unitOfWork.CrmRole.GetItemsByCollection(userCreateRoles, ct: ct);

            User user = new()
            {
                Login = userCreate.Login,
                Name = userCreate.Name,
                Roles = roles,
                Password = hash.Hash(userCreate.Password),
                Active = true
            };

            unitOfWork.User.Create(user);
            await unitOfWork.SaveAsync(ct);

            return NoContent();
        }
    }
}*/