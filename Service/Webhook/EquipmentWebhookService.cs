using CRMService.Interfaces.Repository;
using CRMService.Interfaces.Service;
using CRMService.Models.WebHook;

namespace CRMService.Service.Webhook
{
    public class EquipmentWebhookService(IUnitOfWork unitOfWork) : IWebhookHandler
    {
        public async Task<bool> HandleWebhook(RootEvent @event, CancellationToken ct)
        {
            if (@event.Equipment == null)
                return false;

            switch (@event.Event!.Event_type)
            {
                case "new_equipment":
                    await unitOfWork.Equipment.Upsert(@event.Equipment, ct);
                    break;
                case "change_equipment":
                    await unitOfWork.Equipment.Upsert(@event.Equipment, ct);
                    break;
                default:
                    return false;
            }

            await unitOfWork.SaveAsync(ct);

            return true;
        }

    }
}
