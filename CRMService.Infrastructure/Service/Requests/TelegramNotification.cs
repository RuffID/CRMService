using CRMService.Application.Abstractions.Service;
using CRMService.Application.Models.ConfigClass;
using CRMService.Contracts.Models.Request;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;

namespace CRMService.Infrastructure.Service.Requests
{
    public class TelegramNotification(HttpClient client, IOptions<ApiEndpointOptions> endpoint, IOptions<TelegramBotOptions> settings, ILogger<TelegramNotification> logger) : INotificationService
    {
        private const string AUTHORIZATION_HEADER = "Authorization";

        public async Task SendMessage(long? chatId, string content, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(content) || chatId is null || chatId == 0)
            {
                logger.LogWarning("[Method:{MethodName}] Invalid parameters for Telegram notification. chatId={ChatId}, content length={ContentLength}", nameof(SendMessage), chatId, content?.Length ?? 0);
                return;
            }

            if (string.IsNullOrWhiteSpace(settings.Value.Token))
                throw new InvalidOperationException("TelegramBot:Token is missing in config.json");

            TelegramSendMessageRequest body = new () { Message = content };
            string url = $"{endpoint.Value.TelegramBotUrl}?chatId={chatId}";

            try
            {
                using HttpRequestMessage request = new(HttpMethod.Post, url)
                {
                    Content = JsonContent.Create(body)
                };
                request.Headers.TryAddWithoutValidation(AUTHORIZATION_HEADER, settings.Value.Token);

                using HttpResponseMessage response = await client.SendAsync(request, ct);
                response.EnsureSuccessStatusCode();
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
