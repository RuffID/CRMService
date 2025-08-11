using System.Security.Cryptography;

namespace CRMService.Service.Authorization
{
    public class GenerateRefreshToken
    {
        public string Generate()
        {
            return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        }
    }
}
