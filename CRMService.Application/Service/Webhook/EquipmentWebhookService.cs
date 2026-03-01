using CRMService.Application.Abstractions.Database.Repository;
using CRMService.Domain.Models.OkdeskEntity;
using CRMService.Application.Models.WebHook;
using CRMService.Application.Service.OkdeskEntity;
using CRMService.Application.Service.Sync;

namespace CRMService.Application.Service.Webhook
{
    public class EquipmentWebhookService(IUnitOfWork unitOfWork, EquipmentService service, EntitySyncService sync, ILogger<EquipmentWebhookService> logger) : IWebhookHandler
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
                            await service.CheckInformationOnEquipment(dto, ct);

                            Equipment? existingEquipment = await unitOfWork.Equipment.GetItemByIdAsync(dto.Id, ct: ct);
                            if (existingEquipment == null)
                                unitOfWork.Equipment.Create(dto);
                            else
                                existingEquipment.CopyData(dto);

                            foreach (EquipmentParameter parameter in dto.Parameters)
                            {
                                EquipmentParameter? existingParameter = await unitOfWork.Parameter.GetItemByPredicateAsync(p => p.EquipmentId == parameter.EquipmentId && p.KindParameterId == parameter.KindParameterId, ct: ct);

                                if (existingParameter == null)
                                    unitOfWork.Parameter.Create(parameter);
                                else
                                    existingParameter.CopyData(parameter);
                            }
                            await unitOfWork.SaveChangesAsync(ct);
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