using CRMService.Models.ConfigClass;
using System.Security.Cryptography;

namespace CRMService.Service.Authorization
{
    public class Hasher
    {
        public string Hash(string input)
        {
            byte[] salt = GenerateSalt();
            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
                input,
                salt,
                HashSettingsConstants.ITERATIONS,
                HashAlgorithmName,
                HashSettingsConstants.KEY_SIZE
            );

            return string.Join(
                HashSettingsConstants.SEPARATOR,
                Convert.ToHexString(hash),
                Convert.ToHexString(salt),
                HashSettingsConstants.ITERATIONS,
                HashAlgorithmName
            );
        }        

        private byte[] GenerateSalt() { return RandomNumberGenerator.GetBytes(HashSettingsConstants.SALT_SIZE); }

        private HashAlgorithmName HashAlgorithmName { get { return new HashAlgorithmName(HashSettingsConstants.ALHORITHM); } }
    }
}
