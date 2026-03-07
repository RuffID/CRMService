using CRMService.Application.Abstractions.Database.Repository;
using CRMService.Application.Models.ConfigClass;
using CRMService.Application.Service.Sync;
using CRMService.Domain.Models.OkdeskEntity;
using Microsoft.Extensions.Options;

namespace CRMService.Application.Service.OkdeskEntity
{
    public class KindParameterService(IOptions<ApiEndpointOptions> endpoint, IOptions<OkdeskOptions> okdeskSettings, IOkdeskEntityRequestService request, IUnitOfWork unitOfWork, IOkdeskUnitOfWork okdeskUnitOfWork, EntitySyncService sync, ILogger<KindParameterService> logger)
    {
        public async Task<List<KindsParameter>> GetKindParametersFromCloudApi(CancellationToken ct)
        {
            string link = $"{endpoint.Value.OkdeskApi}/equipments/parameters?api_token={okdeskSettings.Value.OkdeskApiToken}";

            return await request.GetRangeOfItemsAsync<KindsParameter>(link, ct: ct);
        }

        private async Task<List<KindsParameter>> GetKindParametersFromCloudDb(CancellationToken ct)
        {
            List<KindsParameter> parameters = await okdeskUnitOfWork.KindParameter.GetItemsByPredicateAsync(asNoTracking: true, ct: ct);

            return parameters.OrderBy(x => x.Id).ToList();
        }


        public async Task UpdateKindParametersFromCloudApi(CancellationToken ct)
        {
            logger.LogInformation("[Method:{MethodName}] Starting to update kind parameters from API.", nameof(UpdateKindParametersFromCloudApi));

            List<KindsParameter> parameters = await GetKindParametersFromCloudApi(ct);

            if (parameters.Count != 0)
            {
                foreach (KindsParameter item in parameters)
                {
                    await sync.RunExclusive(item, async () =>
                    {
                        KindsParameter? existingTypes = await unitOfWork.KindParameter.GetItemByPredicateAsync(predicate: k => k.Code == item.Code, ct: ct);

                        if (existingTypes == null)
                            unitOfWork.KindParameter.Create(item);
                        else
                            existingTypes.CopyData(item);

                        await unitOfWork.SaveChangesAsync(ct);
                    }, ct);
                }
            }

            logger.LogInformation("[Method:{MethodName}] Update kind parameters completed.", nameof(UpdateKindParametersFromCloudApi));
        }

        public async Task UpdateKindParametersFromCloudDb(CancellationToken ct)
        {
            logger.LogInformation("[Method:{MethodName}] Starting to update kind parameters from DB.", nameof(UpdateKindParametersFromCloudDb));

            List<KindsParameter> parameters = await GetKindParametersFromCloudDb(ct);

            if (parameters.Count != 0)
            {
                foreach (KindsParameter item in parameters)
                {
                    await sync.RunExclusive(item, async () =>
                    {
                        KindsParameter? existingTypes = await unitOfWork.KindParameter.GetItemByPredicateAsync(predicate: k => k.Code == item.Code, ct: ct);

                        if (existingTypes == null)
                            unitOfWork.KindParameter.Create(item);
                        else
                            existingTypes.CopyData(item);

                        await unitOfWork.SaveChangesAsync(ct);
                    }, ct);
                }
            }

            logger.LogInformation("[Method:{MethodName}] Update kind parameters completed.", nameof(UpdateKindParametersFromCloudDb));
        }
    }
}