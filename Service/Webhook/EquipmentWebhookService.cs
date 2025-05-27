using CRMService.Interfaces.Repository;
using CRMService.Interfaces.Service;
using CRMService.Models.Entity;
using CRMService.Models.WebHook;
using CRMService.Service.Entity;
using CRMService.Service.Sync;

namespace CRMService.Service.Webhook
{
    public class EquipmentWebhookService(IUnitOfWorkEntities unitOfWork, EquipmentService equipmentService, EntitySyncService sync) : IWebhookHandler
    {
        public async Task<bool> HandleWebhook(RootEvent @event)
        {
            if (@event.Equipment == null)
                return false;

            switch (@event.Event!.Event_type)
            {
                case "new_equipment":
                    await sync.RunExclusive(async () =>
                    {
                        await equipmentService.CheckInformationOnEquipment(@event.Equipment);
                        await unitOfWork.Equipment.CreateOrUpdate([@event.Equipment]);
                    }, nameof(EquipmentWebhookService));
                    break;
                case "change_equipment":
                    await sync.RunExclusive(async () =>
                    {
                        await equipmentService.CheckInformationOnEquipment(@event.Equipment);
                        await unitOfWork.Equipment.CreateOrUpdate([@event.Equipment]);
                    }, nameof(EquipmentWebhookService));
                    break;
                default:
                    return false;
            }

            await unitOfWork.SaveAsync();

            return true;
        }

    }
}
