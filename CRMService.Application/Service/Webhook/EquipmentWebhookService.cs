using CRMService.Domain.Models.OkdeskEntity;
using CRMService.Application.Models.WebHook;
using CRMService.Application.Service.OkdeskEntity;
using CRMService.Application.Service.Sync;

namespace CRMService.Application.Service.Webhook
{
    public class EquipmentWebhookService(EquipmentService service, EntitySyncService sync, ILogger<EquipmentWebhookService> logger) : IWebhookHandler
    {
        public async Task<bool> HandleWebhook(RootEventWebHook @event, CancellationToken ct)
        {
            if (@event.Equipment == null)
                return false;

            logger.LogInformation("[Method:{MethodName}] Update equipment entity from webhook: \"{EventType}\". Id: {equipmentId}, inventory number: {inventoryNumber}.", nameof(HandleWebhook), @event.Event?.Event_type, @event.Equipment.Id, @event.Equipment.InventoryNumber);

            switch (@event.Event!.Event_type)
            {
                case "new_equipment":
                case "change_equipment":
                    {
                        Equipment dto = @event.Equipment;

                        await sync.RunExclusive(dto, async () =>
                        {
                            await service.CreateOrUpdate(dto, ct);
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