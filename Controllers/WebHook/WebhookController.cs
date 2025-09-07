using Microsoft.AspNetCore.Mvc;
using CRMService.Models.WebHook;
using CRMService.Core.Filter;
using CRMService.Interfaces.Service;
using CRMService.Service.Sync;

namespace CRMService.Controllers.WebHook
{
    [ApiController]
    [Route("api/[controller]")]
    [ServiceFilter(typeof(IpOkdeskWebHookActionFilterAttribute))]
    public class WebhookController(IEnumerable<IWebhookHandler> handlers, EntitySyncService sync) : Controller
    {
        private readonly IEnumerable<IWebhookHandler> _handlers = handlers;

        [HttpPost]
        public async Task<IActionResult> WebHookAction([FromBody] RootEvent @event, CancellationToken ct)
        {
            if (@event?.Event?.Event_type == null)
                return BadRequest("Empty event or action object.");

            await sync.RunExclusive(async () =>
            {
                foreach (IWebhookHandler handler in _handlers)
                {
                    if (await handler.HandleWebhook(@event, ct))
                        break;
                }
            });

            return NoContent();
        }
    }
}