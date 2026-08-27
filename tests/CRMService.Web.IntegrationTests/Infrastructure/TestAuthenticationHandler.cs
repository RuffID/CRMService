using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace CRMService.Web.IntegrationTests.Infrastructure;

public class TestAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string SCHEME_NAME = "CRMService.Test";
    public const string USER_HEADER = "X-Test-User";
    public const string ROLE_HEADER = "X-Test-Role";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(USER_HEADER, out Microsoft.Extensions.Primitives.StringValues userName)
            || string.IsNullOrWhiteSpace(userName))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        List<Claim> claims =
        [
            new(ClaimTypes.NameIdentifier, CrmWebApplicationFactory.TEST_USER_ID.ToString()),
            new(ClaimTypes.Name, userName.ToString())
        ];

        if (Request.Headers.TryGetValue(ROLE_HEADER, out Microsoft.Extensions.Primitives.StringValues role)
            && !string.IsNullOrWhiteSpace(role))
        {
            claims.Add(new Claim(ClaimTypes.Role, role.ToString()));
        }

        ClaimsIdentity identity = new(claims, SCHEME_NAME);
        AuthenticationTicket ticket = new(new ClaimsPrincipal(identity), SCHEME_NAME);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }

    protected override Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        Response.StatusCode = StatusCodes.Status401Unauthorized;
        return Task.CompletedTask;
    }

    protected override Task HandleForbiddenAsync(AuthenticationProperties properties)
    {
        Response.StatusCode = StatusCodes.Status403Forbidden;
        return Task.CompletedTask;
    }
}
