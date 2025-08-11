using CRMService.Models.Authorization;
using System.Text;

namespace CRMService.Service.Authorization
{
    public class GenerateRandomString
    {
        private const string CHARTS = ConstSymbols.UPALPHABET + ConstSymbols.LOWALPHABET + ConstSymbols.NUMBERS + ConstSymbols.SYMBOLS;

        private readonly Random rand = new();
        public string GetString(int length = 12)
        {
            StringBuilder sb = new(length - 1);
            for (int i = 0; i < length; i++)
            {
                sb.Append(CHARTS[rand.Next(0, CHARTS.Length - 1)]);
            }
            return sb.ToString();
        }
    }
}
