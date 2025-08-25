using CRMService.Interfaces.Repository;
using CRMService.Interfaces.Service;
using CRMService.Models.WebHook;
using CRMService.Service.Entity;

namespace CRMService.Service.Webhook
{
    public class CompanyWebhookService(IUnitOfWork unitOfWork, CompanyService companyService) : IWebhookHandler
    {
        public async Task<bool> HandleWebhook(RootEvent @event)
        {
            if (@event.Company == null)
                return false;

            switch (@event.Event!.Event_type)
            {
                case "new_company":
                    if (await companyService.CheckCompanyCategory(@event.Company))
                        await unitOfWork.Company.CreateOrUpdate([@event.Company]);
                    break;

                case "change_company":
                    if (await companyService.CheckCompanyCategory(@event.Company))
                        await unitOfWork.Company.CreateOrUpdate([@event.Company]);
                    break;
                default:
                    return false;
            }

            await unitOfWork.SaveAsync();

            return true;
        }
    }
}