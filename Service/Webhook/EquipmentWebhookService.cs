using CRMService.Interfaces.Repository;
using CRMService.Interfaces.Service;
using CRMService.Models.OkdeskEntity;
using CRMService.Models.WebHook;

namespace CRMService.Service.Webhook
{
    public class EquipmentWebhookService(IUnitOfWork unitOfWork) : IWebhookHandler
    {
        public async Task<bool> HandleWebhook(RootEventWebHook @event, CancellationToken ct)
        {
            if (@event.Equipment == null)
                return false;

            switch (@event.Event!.Event_type)
            {
                case "new_equipment":
                    {
                        Equipment? existingEquipment = await unitOfWork.Equipment.GetItemByIdAsync(@event.Equipment.Id, ct: ct);
                        if (existingEquipment == null)
                            unitOfWork.Equipment.Create(@event.Equipment);
                        else
                            existingEquipment.CopyData(@event.Equipment);
                    }
                    break;
                case "change_equipment":
                    {
                        Equipment? existingEquipment = await unitOfWork.Equipment.GetItemByIdAsync(@event.Equipment.Id, ct: ct);
                        if (existingEquipment == null)
                            unitOfWork.Equipment.Create(@event.Equipment);
                        else
                            existingEquipment.CopyData(@event.Equipment);
                    }
                    break;
                default:
                    return false;
            }

            await unitOfWork.SaveAsync(ct);

            return true;
        }

    }
}
