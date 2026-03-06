using CRMService.Application.Models.WebHook;
using CRMService.Application.Service.OkdeskEntity;
using CRMService.Application.Service.Sync;

namespace CRMService.Application.Service.Webhook
{
    public class CompanyWebhookService(CompanyService companyService, EntitySyncService sync, ILogger<CompanyWebhookService> logger) : IWebhookHandler
    {
        public async Task<bool> HandleWebhook(RootEventWebHook @event, CancellationToken ct)
        {
            if (@event.Company == null)
                return false;

            logger.LogInformation("[Method:{MethodName}] Update company entity from webhook: \"{EventType}\". Id: {companyId}, name: {name}, active: {active}.", nameof(HandleWebhook), @event.Event?.Event_type, @event.Company.Id, @event.Company.Name, @event.Company.Active);

            switch (@event.Event!.Event_type)
            {
                case "new_company":
                case "change_company":
                    await sync.RunExclusive(@event.Company, async () => { await companyService.CreateOrUpdateAsync(@event.Company, ct); }, ct);
                    break;
                default:
                    return false;
            }

            return true;
        }
    }
}