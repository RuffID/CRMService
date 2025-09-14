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
    [Route("api/authorize/[controller]")]
    public class LoginController(IUnitOfWork unitOfWork, IOptions<Models.ConfigClass.AuthorizationOptions> authOptions, ILoggerFactory logger) : Controller
    {
        private readonly Hasher hash = new();
        private readonly JwtTokenService _accessToken = new(authOptions);
        private readonly GenerateRandomString getString = new();
        private readonly ILogger<LoginController> _logger = logger.CreateLogger<LoginController>();

        [HttpPost, AllowAnonymous]
        public async Task<IActionResult> Login([FromQuery] string login, [FromQuery] string password, CancellationToken ct)
        {
            User? user = await unitOfWork.User.GetItemByPredicate(predicate: u => u.Login == login, asNoTracking: true, ct: ct, includes: u => u.Roles);

            if (user == null || user.Active == false || string.IsNullOrEmpty(user.Password) || !hash.Verify(password, user.Password))
                return Unauthorized();

            Token token = new()
            {
                AccessToken = _accessToken.Create(user),
                RefreshToken = getString.GetBase64RandomString()
            };

            Session session = new()
            {
                UserId = user.Id,
                RefreshToken = token.RefreshToken,
                ExpirationRefreshToken = DateTime.UtcNow.AddDays(JWTSettingsConstants.REFRESH_TOKEN_LIFE_TIME_FROM_DAYS)
            };

            unitOfWork.Session.Create(session);
            await unitOfWork.SaveAsync(ct);


            return Ok(token);
        }

        [HttpPut("update_tokens"), AllowAnonymous]
        public async Task<IActionResult> UpdateTokens([FromQuery] string refreshToken, CancellationToken ct)
        {
            Session? session = await unitOfWork.Session.GetItemByPredicate(predicate: s => s.RefreshToken == refreshToken, asNoTracking: false, ct: ct);

            if (session == null) 
                return NotFound();

            User? user = await unitOfWork.User.GetItemById(session.UserId, asNoTracking: true, ct);

            if (user == null)
            {
                _logger.LogError("[Method:{MethodName}] Internal server error, user {userId} not found.", nameof(UpdateTokens), session.UserId);
                return NotFound();
            }

            Token token = new()
            {
                AccessToken = _accessToken.Create(user),
                RefreshToken = getString.GetBase64RandomString()
            };

            session.RefreshToken = token.RefreshToken;
            session.ExpirationRefreshToken = DateTime.UtcNow.AddDays(JWTSettingsConstants.REFRESH_TOKEN_LIFE_TIME_FROM_DAYS);

            await unitOfWork.SaveAsync(ct);
                        
            return Ok(token);
        }        
    }
}
