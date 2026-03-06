using CRMService.Application.Models.WebHook;

namespace CRMService.Application.Abstractions.Service
{
    public interface IWebhookHandler
    {
        Task<bool> HandleWebhook(RootEventWebHook @event, CancellationToken ct);
    }
}