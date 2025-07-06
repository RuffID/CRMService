using AutoMapper;
using CRMService.Interfaces.Repository;
using CRMService.Interfaces.Service;
using CRMService.Models.Entity;
using CRMService.Models.WebHook;
using CRMService.Service.Sync;

namespace CRMService.Service.Webhook
{
    public class MaintenanceEntityWebhookService(IUnitOfWorkEntities unitOfWork, EntitySyncService sync, IMapper mapper) : IWebhookHandler
    {
        public async Task<bool> HandleWebhook(RootEvent @event)
        {
            if (@event.Service_aim == null) 
                return false;

            switch (@event.Event!.Event_type)
            {
                case "new_service_aim":
                    await sync.RunExclusive(async () =>
                    {
                        await unitOfWork.MaintenanceEntity.CreateOrUpdate([mapper.Map<MaintenanceEntity>(@event.Service_aim)]);
                    });
                    break;
                case "change_service_aim":
                    await sync.RunExclusive(async () =>
                    {
                        await unitOfWork.MaintenanceEntity.CreateOrUpdate([mapper.Map<MaintenanceEntity>(@event.Service_aim)]);
                    });
                    break;
                default:
                    return false;
            }

            await unitOfWork.SaveAsync();

            return true;
        }
        
    }
}
