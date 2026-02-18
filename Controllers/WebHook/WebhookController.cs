using CRMService.Abstractions.Service;
using CRMService.Core.Filter;
using CRMService.Models.WebHook;
using CRMService.Service.Sync;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace CRMService.Controllers.WebHook
{
    [ApiController]
    [Route("api/[controller]")]
    [ServiceFilter(typeof(IpOkdeskWebHookActionFilterAttribute))]
    public class WebhookController(IServiceScopeFactory scopeFactory, EntitySyncService sync, ILogger<WebhookController> logger) : Controller
    {
        [HttpPost]
        public async Task<IActionResult> WebHookAction()
        {

            // Для дебага, посмотреть тело запроса в случаи несоответствии моделей либо внутренних ошибок
            string body;
            using (StreamReader reader = new (Request.Body))
                body = await reader.ReadToEndAsync();

            RootEventWebHook? @event;
            try
            {
                @event = JsonConvert.DeserializeObject<RootEventWebHook>(body);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Webhook JSON deserialize failed. Raw body: {Body}", body);
                return BadRequest("Invalid JSON");
            }

            if (@event?.Event?.Event_type == null)
            {
                logger.LogWarning("[Method:{MethodName}] Empty event or action object. Raw body: {Body}", nameof(WebHookAction), body);
                return BadRequest("Empty event or action object.");
            }

            _ = Task.Run(async () =>
            {
                try
                {
                    using IServiceScope scope = scopeFactory.CreateScope();
                    IEnumerable<IWebhookHandler> handlers = scope.ServiceProvider.GetRequiredService<IEnumerable<IWebhookHandler>>();

                    await sync.RunExclusive(async () =>
                    {
                        bool handled = false;
                        foreach (IWebhookHandler handler in handlers)
                        {
                            if (await handler.HandleWebhook(@event, CancellationToken.None))
                            {
                                handled = true;
                                break;
                            }
                        }

                        if (!handled)
                        {
                            logger.LogWarning("[Method:{MethodName}] Webhook NOT handled. Event type: {EventType}. Body: {Body}",
                                nameof(WebHookAction), @event.Event.Event_type, body);
                        }
                    });
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "[Method:{MethodName}] An error occurred while processing the webhook. Event type: {EventType}. Body: {Body}.", nameof(WebHookAction), @event.Event.Event_type, body);
                }
            });

            return NoContent();
        }
    }
}