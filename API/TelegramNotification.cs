using CRMService.Interfaces.Api;
using CRMService.Models.ConfigClass;

namespace CRMService.API
{
    public  class TelegramNotification(IHttpApiClient client, ApiEndpointOptions endpoint)
    {
        public async Task SendMessage(long? chatId, string content, CancellationToken ct = default)
        {
            if (string.IsNullOrEmpty(content) || chatId == null) 
                return;

            object body = new { content };
            string url = $"{endpoint.TelegramBotUrl}?chatId={chatId}";

            await client.PostAsync(url, body, contentType: "application/json", ct: ct);
        }
    }
}
