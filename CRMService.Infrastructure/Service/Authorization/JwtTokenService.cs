using CRMService.Application.Abstractions.Service;
using CRMService.Domain.Models.Authorization;
using CRMService.Application.Models.ConfigClass;
using CRMService.Domain.Models.Constants;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CRMService.Infrastructure.Service.Authorization
{
    public class JwtTokenService(IOptions<AuthorizationOptions> authOptions) : IAccessTokenService
    {
        private readonly AuthorizationOptions _authOptions = authOptions.Value;

        public string Create(User user)
        {
            List<Claim> claims = new();

            foreach (CrmRole role in user.Roles)
                if (!string.IsNullOrEmpty(role.Name))
                    claims.Add(new(ClaimTypes.Role, role.Name));

            JwtSecurityTokenHandler tokenHandler = new ();
            SecurityTokenDescriptor tokenDescriptor = new()
            {                
                Issuer = JWTSettingsConstants.ISSUER,
                Audience = JWTSettingsConstants.AUDIENCE,
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(JWTSettingsConstants.ACCESS_TOKEN_LIFE_TIME_FROM_MINUTES),
                SigningCredentials = new SigningCredentials(SymmetricSecurityKey, SecurityAlgorithms.HmacSha256Signature)
            };            

            SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private SymmetricSecurityKey SymmetricSecurityKey
        {
            get
            {
                if (string.IsNullOrEmpty(_authOptions.JWTSymmetricSecurityKey))
                    throw new InvalidOperationException("Not set symmetric security key in server config.");
                return new(Encoding.UTF8.GetBytes(_authOptions.JWTSymmetricSecurityKey));
            }
        }
    }
}




