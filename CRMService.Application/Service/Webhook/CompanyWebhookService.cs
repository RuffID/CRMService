using CRMService.Application.Abstractions.Database.Repository;
using CRMService.Domain.Models.OkdeskEntity;
using CRMService.Application.Models.WebHook;
using CRMService.Application.Service.OkdeskEntity;

namespace CRMService.Application.Service.Webhook
{
    public class CompanyWebhookService(IUnitOfWork unitOfWork, CompanyService companyService, ILogger<CompanyWebhookService> logger) : IWebhookHandler
    {
        public async Task<bool> HandleWebhook(RootEventWebHook @event, CancellationToken ct)
        {
            if (@event.Company == null)
                return false;

            logger.LogInformation("[Method:{MethodName}] Update company entity from webhook: \"{EventType}\". Id: {companyId}, name: {name}, active: {active}.", nameof(HandleWebhook), @event.Event?.Event_type, @event.Company.Id, @event.Company.Name, @event.Company.Active);

            switch (@event.Event!.Event_type)
            {
                case "new_company":
                case "change_company":
                    if (await companyService.CheckCompanyCategory(@event.Company, ct))
                    {
                        Company? existingCompany = await unitOfWork.Company.GetItemByIdAsync(@event.Company.Id, ct: ct);
                        if (existingCompany == null)
                            unitOfWork.Company.Create(@event.Company);
                        else
                            existingCompany.CopyData(@event.Company);
                    }
                    break;
                default:
                    return false;
            }

            await unitOfWork.SaveChangesAsync(ct);

            return true;
        }
    }
}