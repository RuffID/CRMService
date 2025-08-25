using CRMService.Interfaces.Repository;
using CRMService.Interfaces.Service;
using CRMService.Models.WebHook;
using CRMService.Service.Entity;

namespace CRMService.Service.Webhook
{
    public class EquipmentWebhookService(IUnitOfWork unitOfWork, EquipmentService equipmentService) : IWebhookHandler
    {
        public async Task<bool> HandleWebhook(RootEvent @event)
        {
            if (@event.Equipment == null)
                return false;

            switch (@event.Event!.Event_type)
            {
                case "new_equipment":
                    await equipmentService.CheckInformationOnEquipment(@event.Equipment);
                    await unitOfWork.Equipment.CreateOrUpdate([@event.Equipment]);
                    break;
                case "change_equipment":
                    await equipmentService.CheckInformationOnEquipment(@event.Equipment);
                    await unitOfWork.Equipment.CreateOrUpdate([@event.Equipment]);
                    break;
                default:
                    return false;
            }

            await unitOfWork.SaveAsync();

            return true;
        }

    }
}
