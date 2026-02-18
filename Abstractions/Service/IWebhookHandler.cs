using CRMService.Models.WebHook;

namespace CRMService.Abstractions.Service
{
    public interface IWebhookHandler
    {
        Task<bool> HandleWebhook(RootEventWebHook @event, CancellationToken ct);
    }
}
