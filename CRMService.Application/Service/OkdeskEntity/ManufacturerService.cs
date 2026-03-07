using CRMService.Application.Abstractions.Database.Repository;
using CRMService.Application.Models.ConfigClass;
using CRMService.Application.Service.Sync;
using CRMService.Domain.Models.Constants;
using CRMService.Domain.Models.OkdeskEntity;
using Microsoft.Extensions.Options;
using System.Runtime.CompilerServices;

namespace CRMService.Application.Service.OkdeskEntity
{
    public class ManufacturerService(IOptions<ApiEndpointOptions> endpoint, IOptions<OkdeskOptions> okdeskSettings, IOkdeskEntityRequestService request, IUnitOfWork unitOfWork, IOkdeskUnitOfWork okdeskUnitOfWork, EntitySyncService sync, ILogger<ManufacturerService> logger)
    {
        private async IAsyncEnumerable<List<Manufacturer>> GetManufacturersFromCloudApi(long limit, [EnumeratorCancellation] CancellationToken ct)
        {
            string link = $"{endpoint.Value.OkdeskApi}/equipments/manufacturers?api_token={okdeskSettings.Value.OkdeskApiToken}";

            await foreach (List<Manufacturer> manufacturers in request.GetAllItemsAsync<Manufacturer>(link, startIndex: 0, limit, ct: ct))
                yield return manufacturers;
        }

        private async Task<List<Manufacturer>> GetManufacturersFromCloudDb(CancellationToken ct)
        {
            List<Manufacturer> manufacturers = await okdeskUnitOfWork.Manufacturer.GetItemsByPredicateAsync(asNoTracking: true, ct: ct);
            
            return manufacturers.OrderBy(x => x.Id).ToList();
        }

        public async Task UpdateManufacturersFromCloudApi(CancellationToken ct)
        {
            logger.LogInformation("[Method:{MethodName}] Starting to update model from API.", nameof(UpdateManufacturersFromCloudApi));

            await foreach (List<Manufacturer> manufacturers in GetManufacturersFromCloudApi(LimitConstants.LIMIT_FOR_RETRIEVING_ENTITIES_FROM_API, ct))
            {
                foreach (Manufacturer newManufacturer in manufacturers)
                {
                    await sync.RunExclusive(newManufacturer, async () =>
                    {
                        Manufacturer? existingManufacturer = await unitOfWork.Manufacturer.GetItemByIdAsync(newManufacturer.Id, ct: ct);
                        if (existingManufacturer == null)
                            unitOfWork.Manufacturer.Create(newManufacturer);
                        else
                            existingManufacturer.CopyData(newManufacturer);

                        await unitOfWork.SaveChangesAsync(ct);
                    }, ct);
                }
            }

            logger.LogInformation("[Method:{MethodName}] Update model completed.", nameof(UpdateManufacturersFromCloudApi));
        }

        public async Task UpdateManufacturersFromCloudDb(CancellationToken ct)
        {
            logger.LogInformation("[Method:{MethodName}] Starting to update manufacturers from DB.", nameof(UpdateManufacturersFromCloudDb));

            List<Manufacturer> manufacturers = await GetManufacturersFromCloudDb(ct);

            foreach (Manufacturer newManufacturer in manufacturers)
            {
                await sync.RunExclusive(newManufacturer, async () =>
                {
                    Manufacturer? existingManufacturer = await unitOfWork.Manufacturer.GetItemByIdAsync(newManufacturer.Id, ct: ct);
                    if (existingManufacturer == null)
                        unitOfWork.Manufacturer.Create(newManufacturer);
                    else
                        existingManufacturer.CopyData(newManufacturer);

                    await unitOfWork.SaveChangesAsync(ct);
                }, ct);
            }

            logger.LogInformation("[Method:{MethodName}] Update model completed.", nameof(UpdateManufacturersFromCloudDb));
        }
    }
}
