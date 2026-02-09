using CRMService.Abstractions.Service;
using CRMService.Core.Filter;
using CRMService.Models.WebHook;
using CRMService.Service.Sync;
using Microsoft.AspNetCore.Mvc;

namespace CRMService.Controllers.WebHook
{
    [ApiController]
    [Route("api/[controller]")]
    [ServiceFilter(typeof(IpOkdeskWebHookActionFilterAttribute))]
    public class WebhookController(IEnumerable<IWebhookHandler> handlers, EntitySyncService sync, ILogger<WebhookController> logger) : Controller
    {
        private readonly IEnumerable<IWebhookHandler> _handlers = handlers;

        [HttpPost]
        public async Task<IActionResult> WebHookAction([FromBody] RootEventWebHook @event)
        {
            if (@event?.Event?.Event_type == null)
                return BadRequest("Empty event or action object.");

            await sync.RunExclusive(async () =>
            {
                try
                {
                    foreach (IWebhookHandler handler in _handlers)
                    {
                        if (await handler.HandleWebhook(@event))
                            break;
                    }
                }
                catch (OperationCanceledException)
                {
                    logger.LogWarning("[Method:{MethodName}] Webhook handling was cancelled. Event type: {EventType}.", nameof(WebHookAction), @event.Event.Event_type);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "[Method:{MethodName}] An error occurred while processing the webhook. Event type: {EventType}.", nameof(WebHookAction), @event.Event.Event_type);
                }
            });

            return NoContent();
        }
    }
}