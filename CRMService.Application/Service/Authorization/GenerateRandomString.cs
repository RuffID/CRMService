using CRMService.Domain.Models.Authorization;
using System.Security.Cryptography;
using System.Text;

namespace CRMService.Application.Service.Authorization
{
    public class GenerateRandomString : IRandomStringGenerator
    {
        private const string CHARTS = ConstSymbols.UPALPHABET + ConstSymbols.LOWALPHABET + ConstSymbols.NUMBERS + ConstSymbols.SYMBOLS;

        private readonly Random rand = new();
        public string GetRandomString(int length = 12)
        {
            StringBuilder sb = new(length - 1);
            for (int i = 0; i < length; i++)
            {
                sb.Append(CHARTS[rand.Next(0, CHARTS.Length - 1)]);
            }
            return sb.ToString();
        }

        public string GetBase64RandomString() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));        
    }
}



