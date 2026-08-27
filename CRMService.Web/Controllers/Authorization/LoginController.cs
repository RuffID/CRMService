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
    public class LoginController(AuthenticationService authenticationService) : Controller
    {
        [HttpPost, AllowAnonymous]
        public async Task<IActionResult> Login([FromQuery] string login, [FromQuery] string password, CancellationToken ct)
        {
            Token? token = await authenticationService.LoginAsync(login, password, ct);
            if (token == null)
                return Unauthorized();

            return Ok(token);
        }

        [HttpPut("update_tokens"), AllowAnonymous]
        public async Task<IActionResult> UpdateTokens([FromQuery] string refreshToken, CancellationToken ct)
        {
            Token? token = await authenticationService.UpdateTokensAsync(refreshToken, ct);
            if (token == null)
                return NotFound();

            return Ok(token);
        }
    }
}





