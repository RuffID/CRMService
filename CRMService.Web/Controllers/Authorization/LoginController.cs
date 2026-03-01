using CRMService.Application.Abstractions.Database.Repository;
using CRMService.Application.Abstractions.Service;
using CRMService.Domain.Models.Authorization;
using CRMService.Domain.Models.Constants;
using CRMService.Application.Service.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CRMService.Web.Controllers.Authorization
{
    [Authorize]
    [ApiController]
    [Route("api/authorize/[controller]")]
    public class LoginController(
        IUnitOfWork unitOfWork,
        Hasher hash,
        IAccessTokenService accessTokenService,
        IRandomStringGenerator randomStringGenerator,
        ILogger<LoginController> logger) : Controller
    {
        [HttpPost, AllowAnonymous]
        public async Task<IActionResult> Login([FromQuery] string login, [FromQuery] string password, CancellationToken ct)
        {
            User? user = await unitOfWork.User.GetItemByPredicateAsync(predicate: u => u.Login == login, asNoTracking: true, ct: ct, include: u => u.Include(u => u.Roles));

            if (user == null || user.Active == false || string.IsNullOrEmpty(user.Password) || !hash.Verify(password, user.Password))
                return Unauthorized();

            Token token = new()
            {
                AccessToken = accessTokenService.Create(user),
                RefreshToken = randomStringGenerator.GetBase64RandomString()
            };

            Session session = new()
            {
                UserId = user.Id,
                RefreshToken = token.RefreshToken,
                ExpirationRefreshToken = DateTime.UtcNow.AddDays(JWTSettingsConstants.REFRESH_TOKEN_LIFE_TIME_FROM_DAYS)
            };

            unitOfWork.Session.Create(session);
            await unitOfWork.SaveChangesAsync(ct);

            return Ok(token);
        }

        [HttpPut("update_tokens"), AllowAnonymous]
        public async Task<IActionResult> UpdateTokens([FromQuery] string refreshToken, CancellationToken ct)
        {
            Session? session = await unitOfWork.Session.GetItemByPredicateAsync(predicate: s => s.RefreshToken == refreshToken, asNoTracking: false, ct: ct);

            if (session == null)
                return NotFound();

            User? user = await unitOfWork.User.GetItemByIdAsync(session.UserId, asNoTracking: true, ct: ct);

            if (user == null)
            {
                logger.LogError("[Method:{MethodName}] Internal server error, user {userId} not found.", nameof(UpdateTokens), session.UserId);
                return NotFound();
            }

            Token token = new()
            {
                AccessToken = accessTokenService.Create(user),
                RefreshToken = randomStringGenerator.GetBase64RandomString()
            };

            session.RefreshToken = token.RefreshToken;
            session.ExpirationRefreshToken = DateTime.UtcNow.AddDays(JWTSettingsConstants.REFRESH_TOKEN_LIFE_TIME_FROM_DAYS);

            await unitOfWork.SaveChangesAsync(ct);

            return Ok(token);
        }
    }
}





