using CRMService.Models.Authorization;
using CRMService.Models.ConfigClass;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CRMService.Service.Authorization
{
    public class JwtTokenService(IOptions<AuthorizationOptions> authOptions)
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
                Issuer = _authOptions.Issuer,
                Audience = _authOptions.Audience,
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(_authOptions.AccessTokenLifeTimeFromMinutes),
                SigningCredentials = new SigningCredentials(SymmetricSecurityKey, SecurityAlgorithms.HmacSha256Signature)
            };            

            SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private SymmetricSecurityKey SymmetricSecurityKey
        {
            get
            {
                if (string.IsNullOrEmpty(_authOptions.SymmetricSecurityKey))
                    throw new InvalidOperationException("Not set symmetric security key in server config.");
                return new(Encoding.UTF8.GetBytes(_authOptions.SymmetricSecurityKey));
            }
        }
    }
}
