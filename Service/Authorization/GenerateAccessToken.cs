using CRMService.Models.Authorization;
using CRMService.Models.ConfigClass;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CRMService.Service.Authorization
{
    public class GenerateAccessToken
    {
        private readonly AuthOptions _authOptions;

        public GenerateAccessToken(IOptions<AuthOptions> authOptions)
        {
            _authOptions = authOptions.Value;
        }

        public string? Generate(User user)
        {
            if (string.IsNullOrEmpty(user.Email))
                return null;
            var claims = new List<Claim>();

            if (!string.IsNullOrEmpty(user.Email))
                claims.Add(new(ClaimTypes.Email, user.Email));

            foreach (Role role in user.Roles)
                if (!string.IsNullOrEmpty(role.Name))
                    claims.Add(new(ClaimTypes.Role, role.Name));

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {                
                Issuer = _authOptions.Issuer,
                Audience = _authOptions.Audience,
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(_authOptions.AccessTokenLifeTimeFromMinutes),
                SigningCredentials = new SigningCredentials(SymmetricSecurityKey, SecurityAlgorithms.HmacSha256Signature)
            };            

            var token = tokenHandler.CreateToken(tokenDescriptor);
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
