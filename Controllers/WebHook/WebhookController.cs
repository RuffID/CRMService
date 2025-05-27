using Microsoft.AspNetCore.Mvc;
using CRMService.Models.WebHook;
using CRMService.Core.Filter;
using CRMService.Interfaces.Service;

namespace CRMService.Controllers.WebHook
{    
    [ApiController]
    [Route("api/crm/[controller]")]
    [ServiceFilter(typeof(IpOkdeskWebHookActionFilterAttribute))]
    public class WebhookController(IEnumerable<IWebhookHandler> handlers) : Controller
    {
        private readonly IEnumerable<IWebhookHandler> _handlers = handlers;

        [HttpPost]
        public async Task<IActionResult> WebHookAction([FromBody] RootEvent @event)
        {
            if (@event?.Event?.Event_type == null)
                return BadRequest("Empty event or action object.");

            foreach (IWebhookHandler handler in _handlers)
            {
                if (await handler.HandleWebhook(@event))
                    break;
            }

            return NoContent();
        }
    }
}
