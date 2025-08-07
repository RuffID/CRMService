using System.Security.Cryptography;

namespace CRMService.Service.Authorization
{
    public static class GenerateRefreshToken
    {
        public static string Generate()
        {
            return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        }
    }
}
