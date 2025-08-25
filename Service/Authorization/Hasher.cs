using CRMService.Models.ConfigClass;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;

namespace CRMService.Service.Authorization
{
    public class Hasher
    {
        private readonly HashSettings _hashSettings;

        public Hasher(IOptions<HashSettings> hashSettings)
        {
            _hashSettings = hashSettings.Value;
        }

        public string Hash(string input)
        {
            byte[] salt = GenerateSalt();
            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
                input,
                salt,
                _hashSettings.Iterations,
                HashAlgorithmName,
                _hashSettings.KeySize
            );

            return string.Join(
                _hashSettings.Separator,
                Convert.ToHexString(hash),
                Convert.ToHexString(salt),
                _hashSettings.Iterations,
                HashAlgorithmName
            );
        }        

        private byte[] GenerateSalt() { return RandomNumberGenerator.GetBytes(_hashSettings.SaltSize); }

        private HashAlgorithmName HashAlgorithmName { get { return new HashAlgorithmName(_hashSettings.Algorithm); } }
    }
}
