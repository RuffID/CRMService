using AutoMapper;
using CRMService.Interfaces.Repository;
using CRMService.Interfaces.Service;
using CRMService.Models.Entity;
using CRMService.Models.WebHook;

namespace CRMService.Service.Webhook
{
    public class MaintenanceEntityWebhookService(IUnitOfWork unitOfWork, IMapper mapper) : IWebhookHandler
    {
        public async Task<bool> HandleWebhook(RootEvent @event, CancellationToken ct)
        {
            if (@event.Service_aim == null)
                return false;

            switch (@event.Event!.Event_type)
            {
                case "new_service_aim":
                    await unitOfWork.MaintenanceEntity.Upsert(mapper.Map<MaintenanceEntity>(@event.Service_aim), ct);
                    break;
                case "change_service_aim":
                    await unitOfWork.MaintenanceEntity.Upsert(mapper.Map<MaintenanceEntity>(@event.Service_aim), ct);
                    break;
                default:
                    return false;
            }

            await unitOfWork.SaveAsync(ct);

            return true;
        }

    }
}
