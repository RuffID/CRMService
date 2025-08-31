namespace CRMService.Models.ConfigClass
{
    public class ApiEndpointOptions
    {
        public const string APIENDPOINTS = "ApiEndpoints";
        public string OkdeskDomain { get; set; } = string.Empty;
        public string TelegramBot { get; set; } = string.Empty;
        public string OkdeskApiPath { get; set; } = string.Empty;
        public string OkdeskApi => OkdeskDomain + OkdeskApiPath;
    }
}