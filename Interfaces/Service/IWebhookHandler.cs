using CRMService.Models.WebHook;

namespace CRMService.Interfaces.Service
{
    public interface IWebhookHandler
    {
        Task<bool> HandleWebhook(RootEventWebHook @event, CancellationToken ct);
    }
}
