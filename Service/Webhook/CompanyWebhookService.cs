using CRMService.Interfaces.Repository;
using CRMService.Interfaces.Service;
using CRMService.Models.WebHook;
using CRMService.Service.OkdeskEntity;

namespace CRMService.Service.Webhook
{
    public class CompanyWebhookService(IUnitOfWork unitOfWork, CompanyService companyService) : IWebhookHandler
    {
        public async Task<bool> HandleWebhook(RootEventWebHook @event, CancellationToken ct)
        {
            if (@event.Company == null)
                return false;

            switch (@event.Event!.Event_type)
            {
                case "new_company":
                    if (await companyService.CheckCompanyCategory(@event.Company, ct))
                        await unitOfWork.Company.Upsert(@event.Company, ct);
                    break;

                case "change_company":
                    if (await companyService.CheckCompanyCategory(@event.Company, ct))
                        await unitOfWork.Company.Upsert(@event.Company, ct);
                    break;
                default:
                    return false;
            }

            await unitOfWork.SaveAsync(ct);

            return true;
        }
    }
}