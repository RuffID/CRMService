using CRMService.Interfaces.Repository;
using CRMService.Interfaces.Service;
using CRMService.Models.Dto.Mappers.OkdeskEntity;
using CRMService.Models.WebHook;

namespace CRMService.Service.Webhook
{
    public class MaintenanceEntityWebhookService(IUnitOfWork unitOfWork) : IWebhookHandler
    {
        public async Task<bool> HandleWebhook(RootEventWebHook @event, CancellationToken ct)
        {
            if (@event.Service_aim == null)
                return false;

            switch (@event.Event!.Event_type)
            {
                case "new_service_aim":
                    await unitOfWork.MaintenanceEntity.Upsert(@event.Service_aim.ToEntity(), ct);
                    break;
                case "change_service_aim":
                    await unitOfWork.MaintenanceEntity.Upsert(@event.Service_aim.ToEntity(), ct);
                    break;
                default:
                    return false;
            }

            await unitOfWork.SaveAsync(ct);

            return true;
        }

    }
}
