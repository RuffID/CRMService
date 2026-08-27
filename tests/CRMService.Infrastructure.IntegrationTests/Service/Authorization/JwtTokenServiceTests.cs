using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CRMService.Application.Models.ConfigClass;
using CRMService.Domain.Models.Authorization;
using CRMService.Domain.Models.Constants;
using CRMService.Infrastructure.Service.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace CRMService.Infrastructure.IntegrationTests.Service.Authorization;

public class JwtTokenServiceTests
{
    private const string SIGNING_KEY = "stage-05-signing-key-with-at-least-32-bytes";

    [Fact]
    public void Create_UserRoles_WritesClaimsIssuerAudienceAndExpiration()
    {
        User user = new();
        user.Roles.Add(new CrmRole { Name = "Administrator" });
        user.Roles.Add(new CrmRole { Name = string.Empty });
        JwtTokenService service = CreateService(SIGNING_KEY);

        string encoded = service.Create(user);
        JwtSecurityToken token = new JwtSecurityTokenHandler().ReadJwtToken(encoded);

        Assert.Equal(JWTSettingsConstants.ISSUER, token.Issuer);
        Assert.Contains(JWTSettingsConstants.AUDIENCE, token.Audiences);
        Assert.Contains(token.Claims, claim => claim.Value == "Administrator");
        TimeSpan lifetime = token.ValidTo - DateTime.UtcNow;
        Assert.InRange(lifetime.TotalMinutes, JWTSettingsConstants.ACCESS_TOKEN_LIFE_TIME_FROM_MINUTES - 1, JWTSettingsConstants.ACCESS_TOKEN_LIFE_TIME_FROM_MINUTES + 1);
    }

    [Fact]
    public void Create_ValidSigningKey_ProducesValidSignature()
    {
        string encoded = CreateService(SIGNING_KEY).Create(new User());
        TokenValidationParameters parameters = new()
        {
            ValidateIssuer = true,
            ValidIssuer = JWTSettingsConstants.ISSUER,
            ValidateAudience = true,
            ValidAudience = JWTSettingsConstants.AUDIENCE,
            ValidateLifetime = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SIGNING_KEY))
        };

        ClaimsPrincipal principal = new JwtSecurityTokenHandler().ValidateToken(encoded, parameters, out _);

        Assert.NotNull(principal.Identity);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Create_MissingSigningKey_ThrowsInvalidOperationException(string? key)
    {
        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => CreateService(key).Create(new User()));
        Assert.Contains("symmetric security key", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    private static JwtTokenService CreateService(string? key) =>
        new(Options.Create(new AuthorizationOptions { JWTSymmetricSecurityKey = key! }));
}
