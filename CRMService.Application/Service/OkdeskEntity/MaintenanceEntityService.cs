using CRMService.Application.Abstractions.Database.Repository;
using CRMService.Application.Models.ConfigClass;
using CRMService.Application.Service.Sync;
using CRMService.Domain.Models.Constants;
using CRMService.Domain.Models.OkdeskEntity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Runtime.CompilerServices;

namespace CRMService.Application.Service.OkdeskEntity
{
    public class MaintenanceEntityService(IOptions<ApiEndpointOptions> endpoint,
        IOptions<OkdeskOptions> okdeskSettings, IOkdeskEntityRequestService request, IUnitOfWork unitOfWork, IOkdeskUnitOfWork okdeskUnitOfWork, EntitySyncService sync, ILogger<MaintenanceEntityService> logger)
    {
        public async Task<MaintenanceEntity?> GetMaintenanceEntityFromCloudApi(int maintenanceEntityId, CancellationToken ct)
        {
            string link = $"{endpoint.Value.OkdeskApi}/maintenance_entities/{maintenanceEntityId}?api_token={okdeskSettings.Value.OkdeskApiToken}";

            return await request.GetItemAsync<MaintenanceEntity>(link, ct);
        }

        private async IAsyncEnumerable<List<MaintenanceEntity>> GetMaintenanceEntitiesFromCloudApi(long limit, [EnumeratorCancellation] CancellationToken ct)
        {
            string link = $"{endpoint.Value.OkdeskApi}/maintenance_entities/list?api_token={okdeskSettings.Value.OkdeskApiToken}";
            await foreach (List<MaintenanceEntity> me in request.GetAllItemsAsync<MaintenanceEntity>(link, startIndex: 0, limit, ct: ct))
                yield return me;
        }

        private async Task<List<MaintenanceEntity>> GetMaintenanceEntitiesFromCloudDb(CancellationToken ct)
        {
            List<MaintenanceEntity> maintenanceEntities = await okdeskUnitOfWork.MaintenanceEntity.GetItemsByPredicateAsync(
                asNoTracking: true,
                include: query => query.Include(x => x.Company),
                ct: ct);

            foreach (MaintenanceEntity item in maintenanceEntities)
                item.CompanyId = item.Company?.Id;

            return maintenanceEntities.OrderBy(x => x.Id).ToList();
        }

        public async Task UpdateMaintenanceEntityFromCloudApi(int maintenanceEntityId, CancellationToken ct)
        {
            MaintenanceEntity? newMaintenanceEntity = await GetMaintenanceEntityFromCloudApi(maintenanceEntityId, ct);

            if (newMaintenanceEntity == null)
                return;

            await sync.RunExclusive(newMaintenanceEntity, async () =>
            {
                await CreateOrUpdate(newMaintenanceEntity, ct);
            }, ct);
        }

        public async Task UpdateMaintenanceEntitiesFromCloudApi(CancellationToken ct)
        {
            logger.LogInformation("[Method:{MethodName}] Starting to update maintenance entities from API.", nameof(UpdateMaintenanceEntitiesFromCloudApi));

            await foreach (List<MaintenanceEntity> me in GetMaintenanceEntitiesFromCloudApi(LimitConstants.LIMIT_FOR_RETRIEVING_ENTITIES_FROM_API, ct))
            {
                foreach (MaintenanceEntity newMaintenanceEntity in me)
                {
                    await sync.RunExclusive(newMaintenanceEntity, async () =>
                    {
                        await CreateOrUpdate(newMaintenanceEntity, ct);
                    }, ct);
                }
            }

            logger.LogInformation("[Method:{MethodName}] Update maintenance entities completed.", nameof(UpdateMaintenanceEntitiesFromCloudApi));
        }

        public async Task UpdateMaintenanceEntitiesFromCloudDb(CancellationToken ct)
        {
            logger.LogInformation("[Method:{MethodName}] Starting updating maintenance entities.", nameof(UpdateMaintenanceEntitiesFromCloudDb));

            List<MaintenanceEntity> maintenanceEntities = await GetMaintenanceEntitiesFromCloudDb(ct);

            if (maintenanceEntities.Count == 0)
                return;

            foreach (MaintenanceEntity newMaintenanceEntity in maintenanceEntities)
            {
                await sync.RunExclusive(newMaintenanceEntity, async () =>
                {
                    await CreateOrUpdate(newMaintenanceEntity, ct);
                }, ct);
            }

            logger.LogInformation("[Method:{MethodName}] Maintenance entities update completed.", nameof(UpdateMaintenanceEntitiesFromCloudDb));
        }

        public async Task CreateOrUpdate(MaintenanceEntity maintenanceEntity, CancellationToken ct)
        {
            await CheckMaintenanceEntity(maintenanceEntity, ct);

            MaintenanceEntity? existingMaintenance = await unitOfWork.MaintenanceEntity.GetItemByIdAsync(maintenanceEntity.Id, ct: ct);
            if (existingMaintenance == null)
                unitOfWork.MaintenanceEntity.Create(maintenanceEntity);
            else
                existingMaintenance.CopyData(maintenanceEntity);

            await unitOfWork.SaveChangesAsync(ct);
        }

        private async Task CheckMaintenanceEntity(MaintenanceEntity maintenanceEntity, CancellationToken ct)
        {
            if (maintenanceEntity.Company != null)
            {
                Company? company = await unitOfWork.Company.GetItemByIdAsync(maintenanceEntity.Company.Id, true, ct: ct);
                if (company == null)
                {
                    logger.LogWarning("[Method:{MethodName}] Company with id: {CompanyId} was not found for maintenanceEntity with id: {maintenanceEntityId}.",
                        nameof(CheckMaintenanceEntity), maintenanceEntity.Company.Id, maintenanceEntity.Id);
                    maintenanceEntity.CompanyId = null;
                }
                else
                {
                    maintenanceEntity.CompanyId = company.Id;
                }
            }
            else if (maintenanceEntity.CompanyId.HasValue)
            {
                Company? company = await unitOfWork.Company.GetItemByIdAsync(maintenanceEntity.CompanyId.Value, true, ct: ct);
                if (company == null)
                {
                    logger.LogWarning("[Method:{MethodName}] Company with id: {CompanyId} was not found for maintenanceEntity with id: {maintenanceEntityId}.",
                        nameof(CheckMaintenanceEntity), maintenanceEntity.CompanyId, maintenanceEntity.Id);
                    maintenanceEntity.CompanyId = null;
                }
                else
                {
                    maintenanceEntity.CompanyId = company.Id;
                }
            }

            maintenanceEntity.Company = null;
        }
    }
}