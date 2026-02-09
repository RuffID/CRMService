using CRMService.Models.ConfigClass;
using HttpClientLibrary.Abstractions;

namespace CRMService.API
{
    public  class TelegramNotification(IHttpApiClient client, ApiEndpointOptions endpoint, ILoggerFactory logger)
    {
        private readonly ILogger<TelegramNotification> _logger = logger.CreateLogger<TelegramNotification>();
        public async Task SendMessage(long? chatId, string content, CancellationToken ct = default)
        {
            if (string.IsNullOrEmpty(content) || chatId == null) 
                return;

            object body = new { content };
            string url = $"{endpoint.TelegramBotUrl}?chatId={chatId}";

            try
            {
                await client.PostAsync(url, body, contentType: "application/json", ct: ct);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("[Method:{MethodName}] Telegram notification cancelled. chatId={ChatId}", nameof(SendMessage), chatId );
            }
            catch (HttpRequestException ex)
            {
                _logger.LogWarning("[Method:{MethodName}] Failed to send Telegram notification. chatId={ChatId}, url={Url}, error={Error}", nameof(SendMessage), chatId, url, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Method:{MethodName}] Unexpected error while sending Telegram notification. chatId={ChatId}", nameof(SendMessage), chatId);
            }
        }
    }
}
