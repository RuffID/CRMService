using CRMService.Interfaces.Repository;
using CRMService.Interfaces.Service;
using CRMService.Models.WebHook;
using CRMService.Service.Entity;
using CRMService.Service.Sync;

namespace CRMService.Service.Webhook
{
    public class CompanyWebhookService(IUnitOfWorkEntities unitOfWork, CompanyService companyService, EntitySyncService sync) : IWebhookHandler
    {
        public async Task<bool> HandleWebhook(RootEvent @event)
        {
            if (@event.Company == null)
                return false;

            switch (@event.Event!.Event_type)
            {
                case "new_company":
                    await sync.RunExclusive(async () =>
                    {
                        if (await companyService.CheckCompanyCategory(@event.Company))
                            await unitOfWork.Company.CreateOrUpdate([@event.Company]);
                    });
                    break;

                case "change_company":
                    await sync.RunExclusive(async () =>
                    {
                        if (await companyService.CheckCompanyCategory(@event.Company))
                            await unitOfWork.Company.CreateOrUpdate([@event.Company]);
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