using CRMService.Application.Abstractions.Service;
using CRMService.Application.Models.WebHook;
using CRMService.Web.Core.Filter;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace CRMService.Web.Controllers.WebHook
{
    [ApiController]
    [Route("api/[controller]")]
    [ServiceFilter(typeof(IpOkdeskWebHookActionFilterAttribute))]
    public class WebhookController(IServiceScopeFactory scopeFactory, ILogger<WebhookController> logger) : Controller
    {
        [HttpPost]
        public async Task<IActionResult> WebHookAction()
        {
            // Для дебага, посмотреть тело запроса в случаи несоответствии моделей либо внутренних ошибок
            string body;
            using (StreamReader reader = new(Request.Body))
                body = await reader.ReadToEndAsync();

            RootEventWebHook? @event;
            try
            {
                @event = JsonSerializer.Deserialize<RootEventWebHook>(body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
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




