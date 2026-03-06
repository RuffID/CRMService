using CRMService.Domain.Models.OkdeskEntity;
using CRMService.Application.Models.WebHook;
using CRMService.Application.Common.Mapping.OkdeskEntity;
using CRMService.Application.Service.Sync;
using CRMService.Application.Service.OkdeskEntity;

namespace CRMService.Application.Service.Webhook
{
    public class MaintenanceEntityWebhookService(MaintenanceEntityService service, EntitySyncService sync, ILogger<MaintenanceEntityWebHook> logger) : IWebhookHandler
    {
        public async Task<bool> HandleWebhook(RootEventWebHook @event, CancellationToken ct)
        {
            if (@event.Service_aim == null)
                return false;

            MaintenanceEntityWebHook dto = @event.Service_aim;

            logger.LogInformation("[Method:{MethodName}] Update maintenance entity from webhook: \"{EventType}\" ServiceAim: {serviceAimId}, name: {name}, active: {active}, companyId: {companyId}", nameof(HandleWebhook), @event.Event?.Event_type, @event.Service_aim.Id, @event.Service_aim.Name, @event.Service_aim.Active, @event.Service_aim.Company?.Id);

            switch (@event.Event!.Event_type)
            {
                case "new_service_aim":
                case "change_service_aim":
                    {
                        MaintenanceEntity? maintenanceEntity = @event.Service_aim.ToEntity();

                        await sync.RunExclusive(maintenanceEntity, async () =>
                        {
                            await service.CreateOrUpdate(maintenanceEntity, ct);
                        }, ct);
                    }
                    break;
                default:
                    return false;
            }

            return true;
        }
    }
}