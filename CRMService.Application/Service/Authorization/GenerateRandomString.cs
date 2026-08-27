using CRMService.Domain.Models.Authorization;
using System.Security.Cryptography;
using System.Text;
using CRMService.Application.Abstractions.Service;

namespace CRMService.Application.Service.Authorization
{
    public class GenerateRandomString : IRandomStringGenerator
    {
        private const string CHARTS = ConstSymbols.UPALPHABET + ConstSymbols.LOWALPHABET + ConstSymbols.NUMBERS + ConstSymbols.SYMBOLS;

        public string GetRandomString(int length = 12)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(length);

            StringBuilder sb = new(length);
            for (int i = 0; i < length; i++)
            {
                sb.Append(CHARTS[RandomNumberGenerator.GetInt32(CHARTS.Length)]);
            }
            return sb.ToString();
        }

        public string GetBase64RandomString() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));        
    }
}
