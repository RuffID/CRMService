using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CRMService.Dto.Authorization;
using CRMService.Service.Authorization;
using CRMService.Models.Authorization;
using CRMService.Models.Request;

namespace CRMService.Controllers.Authorization
{
    [Authorize]
    [ApiController]
    [Route("api/authorize/[controller]")]
    public class RegistrationController(RegistrationService _userRegistrationService) : Controller
    {
        [HttpPost, Authorize(Roles = RolesDefinition.ADMIN)]
        public async Task<IActionResult> Registration([FromBody] UserRequestDto user)
        {
            // Если не все поля для создания заполнены, то выдаёт ошибку
            if (string.IsNullOrEmpty(user.Login) || string.IsNullOrEmpty(user.Password) || user.Roles == null || user.Roles.Count == 0)
                return BadRequest("The body with the user was not transferred or not all required fields were filled in.");

            if (!await _userRegistrationService.RegistrationUser(user))
                return StatusCode(500, "Error error while creating user.");            

            return NoContent();
        }
    }
}