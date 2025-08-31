using CRMService.Interfaces.Api;
using CRMService.Models.ConfigClass;
using Newtonsoft.Json;
using System.Text;

namespace CRMService.API
{
    public  class TelegramNotification(IRequestService request, ApiEndpointOptions endpoint, ILoggerFactory logger)
    {
        private readonly ILogger<TelegramNotification> _logger = logger.CreateLogger<TelegramNotification>();

        public async Task SendMessage(long? chatId, string content)
        {
            if (string.IsNullOrEmpty(content) || chatId == null) return;
            string json;
            try
            {
                json = JsonConvert.SerializeObject(content);
            }
            catch (Exception ex) 
            {
                _logger.LogError(ex, "Error while telegram bot sending message.");
                return; 
            }

            await request.SendPost(endpoint.TelegramBot + $"?chatId={chatId}", new StringContent(json, Encoding.UTF8, "application/json"));
        }
    }
}
