using CRMService.Models.Authorization;
using System.Text;

namespace CRMService.Service.Authorization
{
    public class GenerateRandomString
    {
        private const string CHARTS = ConstSymbols.UPALPHABET + ConstSymbols.LOWALPHABET + ConstSymbols.NUMBERS + ConstSymbols.SYMBOLS;

        private static readonly Random rand = new();
        public static string GetString(int length)
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
