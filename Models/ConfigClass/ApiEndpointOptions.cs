namespace CRMService.Models.ConfigClass
{
    public class ApiEndpointOptions
    {
        public const string SectionName = "ApiEndpoints";
        public const string OKDESK_API_PATH = "/api/v1";
        public string TelegramBotUrl { get; set; } = string.Empty;
        public string OkdeskDomainUrl { get; set; } = string.Empty;
        public string OkdeskApi => OkdeskDomainUrl + OKDESK_API_PATH;
    }
}