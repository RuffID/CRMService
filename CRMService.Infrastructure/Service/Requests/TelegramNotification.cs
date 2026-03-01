using CRMService.Application.Abstractions.Service;
using CRMService.Application.Models.ConfigClass;
using CRMService.Contracts.Models.Request;
using HttpClientLibrary.Abstractions;
using Microsoft.Extensions.Options;

namespace CRMService.Infrastructure.Service.Requests
{
    public class TelegramNotification(IHttpApiClient client, IOptions<ApiEndpointOptions> endpoint, ILogger<TelegramNotification> logger) : INotificationService
    {
        public async Task SendMessage(long? chatId, string content, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(content) || chatId is null || chatId == 0)
            {
                logger.LogWarning("[Method:{MethodName}] Invalid parameters for Telegram notification. chatId={ChatId}, content length={ContentLength}", nameof(SendMessage), chatId, content?.Length ?? 0);
                return;
            }

            TelegramSendMessageRequest body = new () { Message = content };
            string url = $"{endpoint.Value.TelegramBotUrl}?chatId={chatId}";

            try
            {
                await client.PostAsync(url, body, ct: ct);
            }
            catch (OperationCanceledException)
            {
                logger.LogWarning("[Method:{MethodName}] Telegram notification cancelled. chatId={ChatId}", nameof(SendMessage), chatId );
            }
            catch (HttpRequestException ex)
            {
                logger.LogWarning("[Method:{MethodName}] Failed to send Telegram notification. chatId={ChatId}, url={Url}, error={Error}", nameof(SendMessage), chatId, url, ex.Message);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "[Method:{MethodName}] Unexpected error while sending Telegram notification. chatId={ChatId}", nameof(SendMessage), chatId);
            }
        }
    }
}