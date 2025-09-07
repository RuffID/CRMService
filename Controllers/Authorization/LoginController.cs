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
    public class LoginController(UserLoginService userLoginService, Hasher hash, IUnitOfWork unitOfWork) : Controller
    {
        [HttpPost, AllowAnonymous]
        public async Task<IActionResult> Login([FromQuery] string login, [FromQuery] string password, CancellationToken ct)
        {
            User? user = await unitOfWork.User.GetItemByPredicate(predicate: u => u.Login == login, asNoTracking: true, ct: ct, includes: u => u.Roles);

            if (user == null || user.Active == false || string.IsNullOrEmpty(user.Password) || !hash.Verify(password, user.Password))
                return Unauthorized("Incorrect login or password.");

            Token token = await userLoginService.LoginInService(user, ct);
         
            return Ok(token);
        }

        [HttpPut("update_tokens"), AllowAnonymous]
        public async Task<IActionResult> UpdateTokens([FromBody] RefreshTokenRequest refresh_token, CancellationToken ct)
        {
            Session? session = await unitOfWork.Session.GetItemByPredicate(predicate: s => s.RefreshToken == refresh_token.RefreshToken, asNoTracking: false, ct: ct);

            if (session == null) 
                return NotFound($"Unable to find session by refresh token - {refresh_token.RefreshToken}.");

            Token token = await userLoginService.UpdateTokens(session, ct);
                        
            return Ok(token);
        }        
    }
}
