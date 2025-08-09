using CRMService.Interfaces.Repository;
using CRMService.Models.Authorization;
using CRMService.Models.ConfigClass;
using CRMService.Service.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace CRMService.Controllers.Authorization
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class LoginController(UserLoginService userLoginService, ILoggerFactory logger, IUnitOfWorkAuthorization unitOfWork, IOptions<HashSettings> hashSettings) : Controller
    {
        private readonly HashVerify hashVerify = new (hashSettings);
        private readonly ILogger<LoginController> _logger = logger.CreateLogger<LoginController>();

        [HttpPost, AllowAnonymous]
        public async Task<IActionResult> Login([FromQuery] string login, [FromQuery] string password)
        {
            // Поиск пользователя
            User? user = await unitOfWork.User.GetItem(new User(login: login));

            if (user == null)
                return Unauthorized("Incorrect login or password.");

            if (string.IsNullOrEmpty(user.PasswordHash))
            {
                _logger.LogError("[Method:{MethodName}] Got user {UserLogin} with blank password, how did this happen?", nameof(Login), user.Login);
                return StatusCode(500, "Internal server error while logging into the service.");
            }

            // Хеширование пароля из запроса и сравнение с хешем из БД
            if (user.Active == false || user.Active == null || !hashVerify.Verify(password, user.PasswordHash))
                return Unauthorized("Incorrect login or password.");

            Token? token = await userLoginService.LoginInService(user);

            if (token == null)
                return StatusCode(500, "Internal server error while logging into the service.");
         
            return Ok(token);
        }

        [HttpPut("update_tokens"), AllowAnonymous]
        public async Task<IActionResult> UpdateTokens([FromBody] RefreshModel refresh_token)
        {
            // Поиск сессии с полученным refresh токеном
            Session? session = await unitOfWork.Session.GetItem(new() { RefreshToken = refresh_token.RefreshToken });

            if (session == null) 
                return NotFound("Invalid refresh token. Session not found.");

            Token? token = await userLoginService.UpdateTokens(session);

            if (token == null)
                return StatusCode(500, "Internal server error while updating tokens.");
                        
            return Ok(token);
        }        
    }
}
