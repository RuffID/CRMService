using CRMService.Models.Authorization;
using CRMService.Models.ConfigClass;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CRMService.Service.Authorization
{
    public class GenerateAccessToken(IOptions<AuthOptions> authOptions)
    {
        private readonly AuthOptions _authOptions = authOptions.Value;

        public string? Generate(User user)
        {
            List<Claim> claims = new();

            if (!string.IsNullOrEmpty(user.Email))
                claims.Add(new(ClaimTypes.Email, user.Email));

            foreach (Role role in user.Roles)
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

        private SymmetricSecurityKey? SymmetricSecurityKey
        {
            get
            {
                if (string.IsNullOrEmpty(_authOptions.SymmetricSecurityKey))
                    return null;
                return new(Encoding.UTF8.GetBytes(_authOptions.SymmetricSecurityKey));
            }
        }
    }
}
