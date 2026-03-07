using CRMService.Application.Abstractions.Database.Repository;
using CRMService.Application.Models.ConfigClass;
using CRMService.Application.Service.Sync;
using CRMService.Domain.Models.Constants;
using CRMService.Domain.Models.OkdeskEntity;
using Microsoft.Extensions.Options;
using System.Runtime.CompilerServices;

namespace CRMService.Application.Service.OkdeskEntity
{
    public class KindService(IOptions<ApiEndpointOptions> endpoint, IOptions<OkdeskOptions> okdeskSettings, IOkdeskEntityRequestService request, IUnitOfWork unitOfWork, IOkdeskUnitOfWork okdeskUnitOfWork, EntitySyncService sync, ILogger<KindService> logger)
    {
        private async IAsyncEnumerable<List<Kind>> GetKindsFromCloudApi(long limit, [EnumeratorCancellation] CancellationToken ct)
        {
            string link = $"{endpoint.Value.OkdeskApi}/equipments/kinds?api_token={okdeskSettings.Value.OkdeskApiToken}";

            await foreach (List<Kind> kinds in request.GetAllItemsAsync<Kind>(link, startIndex: 0, limit, ct: ct))
                yield return kinds;
        }

        private async Task<List<Kind>?> GetKindsFromCloudDb(CancellationToken ct)
        {
            List<Kind> kinds = await okdeskUnitOfWork.Kind.GetItemsByPredicateAsync(asNoTracking: true, ct: ct);

            return kinds.OrderBy(x => x.Id).ToList();
        }

        public async Task UpdateKindsFromCloudApi(CancellationToken ct)
        {
            logger.LogInformation("[Method:{MethodName}] Starting to update kinds from API.", nameof(UpdateKindsFromCloudApi));

            await foreach (List<Kind> kinds in GetKindsFromCloudApi(LimitConstants.LIMIT_FOR_RETRIEVING_ENTITIES_FROM_API, ct))
            {
                if (kinds.Count != 0)
                {
                    foreach (Kind item in kinds)
                    {
                        await sync.RunExclusive(item, async () =>
                        {
                            Kind? existingKinds = await unitOfWork.Kind.GetItemByIdAsync(item.Id, ct: ct);
                            if (existingKinds == null)
                                unitOfWork.Kind.Create(item);
                            else
                                existingKinds.CopyData(item);

                            await unitOfWork.SaveChangesAsync(ct);
                        }, ct);
                    }
                }

                if (kinds.Count < LimitConstants.LIMIT_FOR_RETRIEVING_ENTITIES_FROM_API)
                    break;
            }

            logger.LogInformation("[Method:{MethodName}] Update kinds completed.", nameof(UpdateKindsFromCloudApi));
        }

        public async Task UpdateKindsFromCloudDb(CancellationToken ct)
        {
            logger.LogInformation("[Method:{MethodName}] Starting to update kinds from DB.", nameof(UpdateKindsFromCloudDb));

            List<Kind>? kinds = await GetKindsFromCloudDb(ct);

            if (kinds != null && kinds.Count != 0)
            {
                foreach (Kind item in kinds)
                {
                    await sync.RunExclusive(item, async () =>
                    {
                        Kind? existingKinds = await unitOfWork.Kind.GetItemByIdAsync(item.Id, ct: ct);
                        if (existingKinds == null)
                            unitOfWork.Kind.Create(item);
                        else
                            existingKinds.CopyData(item);

                        await unitOfWork.SaveChangesAsync(ct);
                    }, ct);
                }
            }

            logger.LogInformation("[Method:{MethodName}] Update kinds completed.", nameof(UpdateKindsFromCloudDb));
        }
    }
}