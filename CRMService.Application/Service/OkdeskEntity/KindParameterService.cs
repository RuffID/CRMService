using CRMService.Application.Abstractions.Database.Repository;
using CRMService.Application.Models.ConfigClass;
using CRMService.Application.Service.Sync;
using CRMService.Domain.Models.OkdeskEntity;
using Microsoft.Extensions.Options;
using CRMService.Application.Abstractions.Service;
using Microsoft.Extensions.Logging;

namespace CRMService.Application.Service.OkdeskEntity
{
    public class KindParameterService(IOptions<ApiEndpointOptions> endpoint, IOptions<OkdeskOptions> okdeskSettings, IOkdeskEntityRequestService request, IEquipmentUnitOfWork unitOfWork, IOkdeskEquipmentSource okdeskUnitOfWork, EntitySyncService sync, ILogger<KindParameterService> logger)
    {
        public Task<List<KindsParameter>> GetKindParametersAsync(CancellationToken ct = default) =>
            unitOfWork.KindParameter.GetItemsReadOnlyAsync(ct);

        public async Task<List<KindsParameter>> GetKindParametersFromCloudApi(CancellationToken ct)
        {
            string link = $"{endpoint.Value.OkdeskApi}/equipments/parameters?api_token={okdeskSettings.Value.OkdeskApiToken}";

            return await request.GetRangeOfItemsAsync<KindsParameter>(link, ct: ct);
        }

        private async Task<List<KindsParameter>> GetKindParametersFromCloudDb(CancellationToken ct)
        {
            List<KindsParameter> parameters = await okdeskUnitOfWork.KindParameter.GetAllReadOnlyAsync(ct);

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
                        KindsParameter? existingParameter = await unitOfWork.KindParameter.GetItemByIdAsync(item.Id, ct);

                        if (existingParameter == null)
                            unitOfWork.KindParameter.Create(item);
                        else
                            existingParameter.CopyData(item);

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
                        KindsParameter? existingParameter = await unitOfWork.KindParameter.GetItemByIdAsync(item.Id, ct);

                        if (existingParameter == null)
                            unitOfWork.KindParameter.Create(item);
                        else
                            existingParameter.CopyData(item);

                        await unitOfWork.SaveChangesAsync(ct);
                    }, ct);
                }
            }

            logger.LogInformation("[Method:{MethodName}] Update kind parameters completed.", nameof(UpdateKindParametersFromCloudDb));
        }
    }
}
