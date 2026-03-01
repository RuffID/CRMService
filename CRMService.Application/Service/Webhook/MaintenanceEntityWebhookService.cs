using CRMService.Application.Abstractions.Database.Repository;
using CRMService.Domain.Models.OkdeskEntity;
using CRMService.Application.Models.WebHook;
using CRMService.Application.Common.Mapping.OkdeskEntity;
using CRMService.Application.Service.Sync;

namespace CRMService.Application.Service.Webhook
{
    public class MaintenanceEntityWebhookService(IUnitOfWork unitOfWork, EntitySyncService sync, ILogger<MaintenanceEntityWebHook> logger) : IWebhookHandler
    {
        public async Task<bool> HandleWebhook(RootEventWebHook @event, CancellationToken ct)
        {
            if (@event.Service_aim == null)
                return false;

            MaintenanceEntityWebHook dto = @event.Service_aim;

            logger.LogInformation("[Method:{MethodName}] Update maintenance entity from webhook: \"{EventType}\" ServiceAim: {serviceAimId}, name: {name}, active: {active}, companyId: {companyId}", nameof(HandleWebhook), @event.Event?.Event_type, @event.Service_aim.Id, @event.Service_aim.Name, @event.Service_aim.Active, @event.Service_aim.Company?.Id);

            if (dto.Company == null)
            {
                logger.LogWarning("[Method:{MethodName}] Company information is missing in the webhook payload for MaintenanceEntity with id {MaintenanceEntityId}", nameof(HandleWebhook), dto.Id);
                return true;
            }
            else
            {
                Company? company = await unitOfWork.Company.GetItemByIdAsync(dto.Company.Id, ct: ct);
                if (company == null)
                {
                    logger.LogWarning("[Method:{MethodName}] Company with id {CompanyId} not found for MaintenanceEntity with id {MaintenanceEntityId}", nameof(HandleWebhook), dto.Company.Id, dto.Id);
                    return true;
                }
            }

            switch (@event.Event!.Event_type)
            {
                case "new_service_aim":
                case "change_service_aim":
                    {
                        MaintenanceEntity? existingMe = await unitOfWork.MaintenanceEntity.GetItemByIdAsync(@event.Service_aim.Id, ct: ct);

                        await sync.RunExclusive(@event.Service_aim.ToEntity(), async () =>
                        {
                            if (existingMe == null)
                                unitOfWork.MaintenanceEntity.Create(@event.Service_aim.ToEntity());
                            else
                                existingMe.CopyData(@event.Service_aim.ToEntity());
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