namespace CRMService.Models.ConfigClass
{
    public class ApiEndpoint
    {
        public const string APIENDPOINTS = "ApiEndpoints";
        public string OkdeskDomain { get; set; } = string.Empty;
        public string TelegramBot { get; set; } = string.Empty;
        public string OkdeskApiPath { get; set; } = string.Empty;
        public string OkdeskApi => OkdeskDomain + OkdeskApiPath;
    }
}